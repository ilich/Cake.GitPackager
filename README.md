# Cake.GitPackager
Cake AddIn which copies files based on Git log.

## Example usage
```cs
#addin Cake.GitPackager

GitPackager(
    "C:\project\sample",
    new Dictionary<string, string>()
        {
            { @"src\Views\", @"C:\project\sample\Views\" }
        },
    "1.0.0",
    null
);
```

Cake build will copy all changes happened at *C:\project\sample\src\Views* folder to *C:\project\sample\Views* folder since the changeset tagged with *1.0.0* tag. *C:\project\sample* is a Git repository.