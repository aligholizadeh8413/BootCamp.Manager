# BootCamp.Manager

A simple instructor dashboard for managing bootcamp sessions, students, attendance, assignments, and student performance.

An educational project developed for Session 03 of a C# Backend Bootcamp, focused on ASP.NET Core Web API, Entity Framework Core, CRUD operations, and Clean Architecture.

## Features

- Student and instructor management
- Session management
- Attendance tracking
- Session assignments
- Automatic attendance percentage calculation
- Student performance evaluation
- Course progress tracking
- Persian RTL dark-mode interface

## Tech Stack

| Technology | Purpose |
|---|---|
| C# / .NET 10 | Backend |
| ASP.NET Core Web API | REST API |
| Entity Framework Core | ORM |
| SQLite | Database |
| HTML & CSS | User interface |
| Vanilla JavaScript | Frontend logic |

## Architecture

The project follows a simplified Clean Architecture:

- **Domain:** Entities and enums
- **Application:** Services, DTOs, and interfaces
- **Infrastructure:** EF Core, database, and repositories
- **WebApi:** Controllers, middleware, filters, and static frontend

## Learning Objectives

- REST API and CRUD operations
- HTTP methods and status codes
- Dependency Injection
- Entity Framework Core and migrations
- Controllers, DTOs, and routing
- Middleware and Action Filters
- Layered architecture

## Live Demo

https://aligholizadeh8413.github.io/BootCamp.Manager/

The planned GitHub Pages demo is a static frontend preview using sample data. The backend and SQLite database run locally.

## Previous Practice

https://github.com/aligholizadeh8413/CSharp.CRUD.Practice

## Project Status

Educational project — under development.
