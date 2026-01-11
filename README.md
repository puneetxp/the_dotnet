# The DotNet Library

A standard library for .NET Core applications, ported from `the_lib` (PHP) and `the_deno`. This library provides core utilities for creating robust backend services with ease.

## Features

- **SqlBuilder**: Dynamic SQL generation with efficient `Left Join` support.
- **Model**: Base ORM class providing fluent API for `All`, `Find`, `Create`, `Update`, `Delete`, and `Join`.
- **Auth**: Utilities for building Login, Register, and Password Hashing (compatible with standard schemes).
- **FileAct**: helper for handling file uploads, directory management, and public path mapping.
- **Mail**: Wrapper for sending emails (SMTP).
- **Response**: Standardized JSON response envelopes (`Json`, `NotFound`, `Unauthorized`).
- **Session**: Abstraction for session state management.

## Installation

Add the library to your solution.

```bash
dotnet add reference path/to/The.DotNet.Lib.csproj
```

## Usage

### 1. Database & Models

Define your model extending the base `Model` class.

```csharp
using The.DotNet.Lib;

public class User : Model
{
    public User(IDB db) : base(db) 
    {
        this.Table = "users";
    }
}

// Usage in Controller
var userModel = new User(db);
var users = userModel.Join(new Dictionary<string, dynamic> {
    { "roles", "roles" } // Simple join
}).All();
```

### 2. Authentication

```csharp
var result = Auth.Login(email, password);
```

### 3. File Upload

```csharp
var fileAct = FileAct.Init("file_input", "../storage");
var uploaded = fileAct.Upload(formFile);
```
