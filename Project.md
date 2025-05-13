✅ Project Description :
I am currently working on a project in QapMap, a platform used to manage cap tables—documents that outline a company’s ownership structure, listing all shareholders and their respective ownership percentages. We assist startups by providing a detailed view of their ESOPs and equity. Stakeholders can include both investors and employees. The platform supports features like share buybacks, vesting, surrender, and exercise of equity instruments.

✅ Tech Stack and One-Liner Benefits:
	•	C# / .NET: A modern, high-performance framework for building scalable, maintainable backend services.
	•	Event Store: Enables reliable event sourcing by storing the full history of state changes for auditability and traceability.
	•	MongoDB: A flexible NoSQL database ideal for handling dynamic and hierarchical data structures like cap tables.
	•	AutoMapper: Simplifies object-to-object mapping, reducing boilerplate code in DTO conversions.
	•	CQRS Pattern: Separates read and write logic for better performance, scalability, and maintainability.
	•	Docker: Provides lightweight containers for consistent development, testing, and deployment across environments.
	•	NServiceBus: Handles asynchronous messaging and distributed workflows for resilient and decoupled microservices.

**Event store vs Mongodb*
We use Event Store for writes and MongoDB for reads to implement the CQRS pattern. This approach lets us optimize each database for its purpose—Event Store captures every change as an immutable event for auditability and traceability, while MongoDB is tailored for fast, flexible reads. It also improves performance, as read and write workloads are decoupled and can be scaled independently, making the system more resilient and maintainable.”

