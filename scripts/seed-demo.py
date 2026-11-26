"""Insert fictional examples through the real API into an EMPTY database.
Usage: python3 scripts/seed-demo.py [http://localhost:5083]
"""
import json
import sys
import urllib.request

base = (sys.argv[1] if len(sys.argv)>1 else 'http://localhost:5083').rstrip('/')
def request(path, data=None):
    body = None if data is None else json.dumps(data).encode()
    req = urllib.request.Request(base+'/api/'+path, body, {'Content-Type':'application/json'})
    with urllib.request.urlopen(req, timeout=15) as response:
        text=response.read()
        return json.loads(text) if text else None
for resource in ['students','instructors','sessions','evaluations']:
    if request(resource):
        raise SystemExit('دیتابیس خالی نیست؛ هیچ داده‌ای اضافه نشد.')
teacher=request('instructors',{'fullName':'آرش محمدی','email':'arash@example.com','expertise':'ASP.NET Core','isActive':True})
names=['علی رضایی','سارا احمدی','محمد کریمی','نگار محمدی','رضا حسینی','مریم عباسی','امیر نادری','نازنین صادقی','حسین موسوی','زهرا مرادی','پویا اکبری','هانیه کاظمی']
students=[request('students',{'fullName':name,'email':f'student{i+1}@example.com','isActive':True}) for i,name in enumerate(names)]
titles=['مبانی C# و شیءگرایی','آشنایی با Entity Framework','Web API و EF Core','معماری Web API','Middleware و Action Filter','معماری لایه‌ای و سرویس‌ها']
tasks=['تمرین کلاس و آبجکت','مدیریت کتاب‌ها','ساخت پروژه CRUD','ساخت Service Layer','ثبت لاگ درخواست','تکمیل پنل بوت‌کمپ']
for i,title in enumerate(titles):
    session=request('sessions',{'title':title,'date':f'2026-11-{14+i*2:02}','instructorId':teacher['id'],'taskDescription':tasks[i],'progressPercent':[15,30,45,65,80,0][i],'isCompleted':i<5,'studentIds':[s['id'] for s in students]})
    if i<5:
        body=json.dumps({'items':[{'studentId':s['id'],'status':'Present' if j<[11,10,12,10,11][i] else 'Absent'} for j,s in enumerate(students)]}).encode()
        req=urllib.request.Request(base+f"/api/sessions/{session['id']}/attendance",body,{'Content-Type':'application/json'},method='PUT')
        with urllib.request.urlopen(req) as response: response.read()
print('۱۲ دانشجو، ۱ مدرس و ۶ جلسه با حضور نمونه در دیتابیس ثبت شد.')
