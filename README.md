# TechNova.RMS CLI

Overview 🧭
--------

The CLI project is a lightweight command-line interface for the TechNova.RMS system. It demonstrates how the application uses abstractions to separate concerns: the CLI calls into the application/service layer, which depends on repository abstractions implemented by the Infra layer. The project targets .NET 10 and is intended for local, single-process scenarios.

Project status & roadmap 🗺️
-------------------------

- Important: TechNova.RMS is designed as an API project, but the API implementation is not yet complete. The current repository milestone focused on implementing and validating the CLI surface, service-layer logic, and repository abstractions.
- Next milestone: complete the API layer for TechNova.RMS and, if bandwidth permits, implement a UI that consumes the API. The CLI will continue to serve as the primary integration and testing surface until the API/UI milestones are completed.

How abstractions are used
-------------------------

- Repository abstraction (IRepository / IEmployeeRepository) 🗃️
  - The Infra layer implements repository interfaces that encapsulate persistence operations (CRUD, queries). The rest of the app depends on these interfaces rather than concrete classes, so persistence can be swapped (JSON file, database, etc.) without changing business logic.

- Service abstraction (IService / IEmployeeService) 🔁
  - The service layer contains business logic and exposes service interfaces used by the CLI. Services perform validation, coordinate repositories, and raise domain events or notifications as needed. The CLI interacts only with service interfaces, keeping the UI thin and focused on input/output.

- Data store abstraction (IDataStore / IJsonDataStore) 🗄️
  - The Infra layer uses a small data-store abstraction to hide details of reading/writing the JSON file. This keeps file handling, serialization, and concurrency concerns isolated from repository and service code.

Typical call flow
-----------------

CLI -> IEmployeeService -> IEmployeeRepository -> JsonDataStore (Infra)

This flow keeps each layer focused: the CLI handles I/O, services handle business rules and transactions, repositories handle data access, and the data store handles serialization and file persistence.

Change tracking and persistence
-------------------------------

- ObservableCollection<T> is used for in-memory collections that need change notifications. The Infra layer exposes collections or provides events so services (or UI) can subscribe to CollectionChanged events.
- When a collection change occurs (add/remove/replace), the Infra layer or a registered handler persists the corresponding change back to the JSON file so the file remains the source of truth. This pattern keeps runtime consumers in sync with persisted data without a full RDBMS.

Notes and caveats ⚠️
---------------------

- The JSON-backed approach is best for single-process, low-concurrency scenarios. For multi-process or distributed use, migrate to a proper database or add file-locking and concurrency/version checks.
- Keep business logic in services, not in the CLI or Infra. The CLI should remain a thin adapter.

Planned improvements 🚀
---------------------

- We will update and optimize the bulk-fetch code in the database/infra layer to improve performance and memory usage for large datasets. This refactor will keep the repository and service contracts stable while replacing internal data-loading strategies.

Getting started
---------------

1. Build the solution: dotnet build
2. Run tests: dotnet test
3. Run the CLI: open a terminal in the CLI project and run `dotnet run` or start from Visual Studio.

Contributing
------------

When you change persistence behavior or repository contracts, update the README and the Infra documentation so other contributors understand migration and compatibility concerns.

License
-------

See solution repository for licensing information.
