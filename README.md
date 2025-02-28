**Project Overview**

This project is built using EventStoreDB, MongoDB, NService Bus and the CQRS pattern with a Domain-Driven Design (DDD) approach. It follows an event-driven architecture and is developed using C# .NET 9.

For logging, we use SEQ to collect and visualize application logs.

All required Docker images are defined in a Docker Compose YAML file, which is used to run the application efficiently.

Mapper Concept:

We use a mapping layer to convert data between different representations in the system:
1.	Event Mapping – Transforms raw EventStoreDB events into domain objects.
2.	Domain to Read Model Mapping – Converts domain entities into MongoDB projection views for efficient querying.
3.	DTO Mapping – Translates domain models into DTOs (Data Transfer Objects) for API responses.

This approach ensures separation of concerns, improves maintainability, and enables efficient querying by structuring data in a format optimized for reads.

Project Flow
1.	When an API request reaches the controller, we use a mapper to convert the incoming object from the API layer to the application layer.
2.	We use NServiceBus, which sends a command to the command handler, triggering the necessary operations.
3.	Every operation results in an event, which is then created and stored in the repository.
4.	EventStoreDB acts as the source of truth for all events.
5.	When a new event arrives, we fetch the checkpoint of the repository after every three events to optimize processing and we stores those in separate stream.
6.	We load the last known state from the checkpoint and apply the next event for efficiency.


Projection Flow
1.	We run a background service that listens to the event stream we have subscribed to in EventStoreDB.
	When EventStoreDB publishes an event, the background service listens for it and pushes it to a change log stream.
2.	From the change log stream, the events are transferred to the tenant log stream, which contains tenant-specific events.
	These events are then processed and sent to the projection system, where they are mapped from EventStoreDB to MongoDB.
	We maintain a checkpoint of the last processed event to ensure efficient event processing and prevent duplication.
3.	The projection process updates MongoDB with the latest state, ensuring that all data is synchronized and query-optimized.

Tracing and Logging

We use the concept of middleware with a Correlation ID to trace all requests throughout the system.
•	Every incoming request is assigned a Correlation ID, which helps in tracking the entire request flow across different services.
•	This ensures better observability and debugging by linking logs to specific requests.
•	The log state is also captured, allowing us to analyze system behavior and troubleshoot issues efficiently.