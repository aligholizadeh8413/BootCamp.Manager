"""Integration test: real Kestrel + EF Core + a temporary SQLite file. Python stdlib only.
Run from the repository root AFTER dotnet build: python3 tests/smoke.py
Never touches the user's App_Data database.
"""
import html
import http.cookiejar
import json
import os
from pathlib import Path
import re
import socket
import sqlite3
import subprocess
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / 'src/BootCamp.Manager.WebApi'
DLL = PROJECT / 'bin/Debug/net10.0/BootCamp.Manager.WebApi.dll'
DOTNET = os.environ.get('BOOTCAMP_DOTNET', 'dotnet')
checks = []

def check(condition, message):
    assert condition, message
    checks.append(message)
    print('PASS', message, flush=True)

def main():
    with socket.socket() as sock:
        sock.bind(('127.0.0.1', 0))
        port = sock.getsockname()[1]
    base = f'http://127.0.0.1:{port}'
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
    def request(method, path, body=None, expected=200, form=False):
        data = None if body is None else (urllib.parse.urlencode(body).encode() if form else json.dumps(body).encode())
        headers = {} if data is None else {'Content-Type': 'application/x-www-form-urlencoded' if form else 'application/json'}
        req = urllib.request.Request(base + path, data, headers, method=method)
        try:
            response = opener.open(req, timeout=10)
        except urllib.error.HTTPError as error:
            response = error
        raw = response.read().decode()
        assert response.status == expected, f'{method} {path}: wanted {expected}, got {response.status}: {raw}'
        return (json.loads(raw) if 'json' in response.headers.get('Content-Type', '') and raw else raw), response.headers
    with tempfile.TemporaryDirectory(prefix='bootcamp-smoke-') as directory:
        dbfile = Path(directory) / 'test.db'
        logpath = Path(directory) / 'server.log'
        env = dict(os.environ, ConnectionStrings__Bootcamp=f'Data Source={dbfile};Foreign Keys=True', ASPNETCORE_ENVIRONMENT='Development')
        process = None
        with logpath.open('w+') as log:
            def start():
                p = subprocess.Popen([DOTNET, str(DLL), '--urls', base], cwd=PROJECT, env=env, stdout=log, stderr=log)
                for _ in range(150):
                    if p.poll() is not None:
                        log.flush(); raise RuntimeError(logpath.read_text())
                    try:
                        request('GET', '/api/students'); return p
                    except (urllib.error.URLError, ConnectionError): time.sleep(.1)
                p.terminate(); raise RuntimeError('Server readiness timeout')
            try:
                process = start()
                check(dbfile.exists(), 'Migration creates a new SQLite database')
                with sqlite3.connect(dbfile) as connection:
                    check(connection.execute('SELECT COUNT(*) FROM __EFMigrationsHistory').fetchone()[0] == 1, 'Initial migration recorded exactly once')
                page, _ = request('GET', '/')
                check('js/app.js' in page and 'dir="rtl"' in page, 'wwwroot serves the RTL application')
                ins = {'fullName':'مدرس تست','email':'teacher@example.com','expertise':'C#','isActive':True}
                teacher, headers = request('POST','/api/instructors',ins,201)
                tid = teacher['id']
                check(headers['Location'].endswith(f'/api/instructors/{tid}'), 'POST instructor returns 201 and Location')
                ins['expertise'] = 'ASP.NET Core'
                request('PUT',f'/api/instructors/{tid}',ins,204)
                read,_ = request('GET',f'/api/instructors/{tid}')
                check(read['expertise'] == 'ASP.NET Core', 'Instructor update persisted')
                student = {'fullName':'دانشجوی اول','email':'first@example.com','isActive':True}
                first,_ = request('POST','/api/students',student,201); sid=first['id']
                second,_ = request('POST','/api/students',dict(student,fullName='دانشجوی دوم',email='second@example.com'),201)
                student['fullName'] = 'نام ویرایش شده'
                request('PUT',f'/api/students/{sid}',student,204)
                read,_ = request('GET',f'/api/students/{sid}')
                check(read['fullName'] == student['fullName'], 'Student create/read/update')
                request('POST','/api/students',dict(student,fullName='   '),400)
                request('POST','/api/students',dict(student,email='invalid'),400)
                request('POST','/api/students',dict(student,fullName='x'*101,email='long@example.com'),400)
                check(True, 'Whitespace, invalid email and excessive length rejected')
                problem,_=request('POST','/api/students',dict(student,email='FIRST@example.com'),409)
                check(problem.get('traceId') is not None, 'Duplicate email rejected with ProblemDetails traceId')
                request('GET','/api/students/999999',expected=404)
                request('PUT','/api/students/999999',student,404)
                request('DELETE','/api/students/999999',expected=404)
                check(True, 'Missing student GET/PUT/DELETE returns 404')
                session={'title':'جلسه تست','date':'2026-11-26','instructorId':tid,'taskDescription':'تمرین CRUD','progressPercent':40,'isCompleted':True,'studentIds':[sid,second['id']]}
                created,_=request('POST','/api/sessions',session,201); session_id=created['id']
                check(created['participantCount']==2 and created['pendingCount']==2 and created['attendancePercent']==0, 'Session starts with its own pending roster')
                third,_=request('POST','/api/students',dict(student,email='third@example.com'),201)
                read,_=request('GET',f'/api/sessions/{session_id}')
                check(read['participantCount']==2, 'Adding a student does not change the old session roster')
                attendance={'items':[{'studentId':sid,'status':'Present'},{'studentId':second['id'],'status':'Absent'}]}
                request('PUT',f'/api/sessions/{session_id}/attendance',attendance,204)
                read,_=request('GET',f'/api/sessions/{session_id}')
                check(read['attendancePercent']==50 and read['pendingCount']==0, 'Attendance is calculated from the session roster (50%)')
                for bad in [
                    {'items':[{'studentId':sid,'status':'Present'}]},
                    {'items':[{'studentId':sid,'status':'Present'},{'studentId':sid,'status':'Absent'}]},
                    {'items':[{'studentId':sid,'status':'Present'},{'studentId':third['id'],'status':'Absent'}]},
                    {'items':[{'studentId':sid,'status':'Unknown'},{'studentId':second['id'],'status':'Absent'}]},
                    {'items':[{'studentId':sid,'status':1},{'studentId':second['id'],'status':'Absent'}]},
                    {'items':[None]}, {'items':None}
                ]: request('PUT',f'/api/sessions/{session_id}/attendance',bad,400)
                read,_=request('GET',f'/api/sessions/{session_id}')
                check(read['attendancePercent']==50, 'Invalid attendance batches are rejected atomically')
                attendance['items'][1]['status']='Present'
                request('PUT',f'/api/sessions/{session_id}/attendance',attendance,204)
                request('PUT',f'/api/sessions/{session_id}/attendance',attendance,204)
                read,_=request('GET',f'/api/sessions/{session_id}')
                check(read['attendancePercent']==100, 'Attendance editing and repeated PUT are idempotent')
                update=dict(session,title='جلسه ویرایش شده',progressPercent=60);del update['studentIds']
                request('PUT',f'/api/sessions/{session_id}',update,204)
                read,_=request('GET',f'/api/sessions/{session_id}')
                check(read['title']==update['title'] and read['progressPercent']==60 and read['participantCount']==2, 'Session update preserves roster and saves task/progress')
                for bad in [dict(session,progressPercent=101),dict(session,instructorId=999999),dict(session,studentIds=[sid,sid]),dict(session,studentIds=[999999]),dict(session,date=None),dict(session,date='0001-01-01')]:
                    request('POST','/api/sessions',bad,400)
                check(True, 'Session progress/date/instructor/roster validation')
                empty,_=request('POST','/api/sessions',dict(session,studentIds=[]),201)
                check(empty['attendancePercent'] is None, 'Empty session attendance is null (not division by zero)')
                request('DELETE',f"/api/sessions/{empty['id']}",expected=204)
                request('DELETE',f'/api/students/{sid}',expected=409)
                request('DELETE',f'/api/instructors/{tid}',expected=409)
                student['isActive']=False
                request('PUT',f'/api/students/{sid}',student,204)
                read,_=request('GET',f'/api/sessions/{session_id}')
                check(read['participantCount']==2 and read['attendancePercent']==100, 'History-protected deletion and deactivation preserve statistics')
                request('POST','/api/sessions',session,400)
                check(True, 'Inactive student cannot be selected for a new session')
                evaluation={'sessionId':session_id,'studentId':sid,'instructorId':tid,'score':85,'notes':'توضیحات تست'}
                ev,_=request('POST','/api/evaluations',evaluation,201); eid=ev['id']
                check(ev['studentName']==student['fullName'], 'Evaluation create returns related names')
                request('POST','/api/evaluations',evaluation,409)
                request('POST','/api/evaluations',dict(evaluation,studentId=third['id']),400)
                request('PUT',f'/api/evaluations/{eid}',dict(evaluation,score=-1),400)
                request('PUT',f'/api/evaluations/{eid}',dict(evaluation,score=101),400)
                request('PUT',f'/api/evaluations/{eid}',dict(evaluation,instructorId=999999),400)
                check(True, 'Evaluation duplicate, nonparticipant, score and instructor validation')
                evaluation.update(score=95,notes='ویرایش ارزیابی')
                request('PUT',f'/api/evaluations/{eid}',evaluation,204)
                read,_=request('GET',f'/api/evaluations/{eid}')
                check(read['score']==95 and read['notes']==evaluation['notes'], 'Evaluation update persists')
                for endpoint in ['students','instructors','sessions','evaluations']:
                    data,_=request('GET',f'/api/{endpoint}')
                    check(isinstance(data,list) and len(data)>0,f'GET /api/{endpoint} returns persisted rows')
                page,_=request('GET','/learning/mvc')
                token=html.unescape(re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',page).group(1))
                page,_=request('POST','/learning/mvc',{'Name':'علی','__RequestVerificationToken':token},form=True)
                check('سلام' in html.unescape(page) and 'علی' in html.unescape(page), 'MVC form binding and server-rendered View')
                page,_=request('POST','/learning/mvc',{'Name':'   ','__RequestVerificationToken':token},form=True)
                check('نام را وارد کنید' in html.unescape(page), 'MVC validation returns field error')
                request('POST','/learning/mvc',{'Name':'Ali'},expected=400,form=True)
                check(True, 'MVC form rejects missing antiforgery token')
                process.terminate(); process.wait(timeout=10)
                process=start()
                read,_=request('GET',f'/api/evaluations/{eid}')
                session_read,_=request('GET',f'/api/sessions/{session_id}')
                check(read['score']==95 and session_read['attendancePercent']==100, 'Data survives a real process restart')
                with sqlite3.connect(dbfile) as connection:
                    check(connection.execute('PRAGMA foreign_key_check').fetchall()==[], 'SQLite foreign keys remain consistent')
                    check(connection.execute('SELECT COUNT(*) FROM __EFMigrationsHistory').fetchone()[0]==1, 'Restart does not reapply the migration')
                request('DELETE',f'/api/evaluations/{eid}',expected=204)
                request('GET',f'/api/evaluations/{eid}',expected=404)
                request('POST','/api/evaluations',evaluation,201)
                request('DELETE',f'/api/sessions/{session_id}',expected=204)
                data,_=request('GET','/api/evaluations')
                check(data==[], 'Deleting session cascades to its evaluations')
                with sqlite3.connect(dbfile) as connection:
                    check(connection.execute('SELECT COUNT(*) FROM SessionParticipants').fetchone()[0]==0, 'Deleting session cascades to its participants')
                for st in [first,second,third]: request('DELETE',f"/api/students/{st['id']}",expected=204)
                request('DELETE',f'/api/instructors/{tid}',expected=204)
                check(True, 'Student/instructor/evaluation/session DELETE works after references are removed')
                seed = subprocess.run(['python3', str(ROOT/'scripts/seed-demo.py'), base], capture_output=True, text=True)
                assert seed.returncode == 0, seed.stderr
                seeded_students,_ = request('GET','/api/students')
                seeded_sessions,_ = request('GET','/api/sessions')
                check(len(seeded_students)==12 and len(seeded_sessions)==6, 'Optional seed script creates database-backed example data')
                refused = subprocess.run(['python3', str(ROOT/'scripts/seed-demo.py'), base], capture_output=True, text=True)
                after_seed,_ = request('GET','/api/students')
                check(refused.returncode != 0 and len(after_seed)==12, 'Seed refuses a nonempty database without adding duplicates')
                log.flush(); logs=logpath.read_text()
                check('HTTP GET /api/students' in logs and 'Action starting:' in logs and 'Action finished:' in logs, 'HTTP middleware and action filter emit separate logs')
            finally:
                if process is not None and process.poll() is None: process.terminate(); process.wait(timeout=10)
    print(f'\n{len(checks)} checks passed. Temporary database removed.')
if __name__=='__main__': main()
