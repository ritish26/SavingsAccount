# 1.  **Why are you using docker in your application?**
 - We containerized our .NET application using Docker to streamline deployments and avoid machine-specific issues.
 - Each service has its own Dockerfile, and we use Docker Compose for local orchestration. 
 - This setup also allowed us to easily integrate with our Azure DevOps CI/CD pipelines.
 - This means:
   Whether you’re running the app on your laptop, a test server, or a production server, Docker makes sure it behaves 
   the same way — no more “it works on my machine” problems.

# 2. **What is Event sourcing**
Event sourcing is a design pattern where instead of storing just the current state of data in a database, 
we store a sequence of events that led to that state. Every change in the system is captured as an event, and the current state is rebuilt by replaying those events.
In a bank system, instead of just storing the current balance, we store events like:
•	AccountCreated
•	MoneyDeposited
•	MoneyWithdrawn

Why use Event Sourcing?
1. Full audit history — you know exactly what happened and when
2. Easy rollback — replay events to a past state
3. Debugging and testing — reproduce bugs by replaying events
4. Rebuild projections — like summaries, reports, or views anytime

**Event Publishing**
- Once events are stored in an event store, these events could be published to an Event Broker for its subscribers to consume them further. 
You can use an Event Broker (i.e. Apache Kafka) to handle this situation.
In this, each microservice is subscribed to events published by other microservices, where there is no central orchestration.

# 3. **How is Event Sourcing different from traditional CRUD?**

In traditional CRUD, we store only the current state of an entity in a database. Every time an update happens, we overwrite the existing data.
In event sourcing, instead of storing just the current state, we store a sequence of events that represent every change to the state. The current state is rebuilt by replaying these events.

# 4. **How do you handle schema changes in events?**
In event sourcing, once events are stored, they’re immutable — we can’t change them. So when the schema of an event evolves,
we handle it by introducing versioning. There are a few common strategies like adding new fields, creating new event versions, or
using upcasters to transform old events into the current format at read time.

a. **Event Versioning:**
* Create a new version of the event type.
* Example: UserCreatedV1 → UserCreatedV2
* keep both versions supported in your codebase.

b. **Additive Changes (non-breaking):**
* Add new optional fields to the event.
* Existing consumers can ignore the new fields.
* Most commonly used for backward-compatible updates.

# 5. **ACID in microservices**
ACID stands for Atomicity, Consistency, Isolation, and Durability, and it applies to database transactions. In a monolithic app, 
it’s easy to maintain ACID because all data is in one database.
But in microservices, data is split across services, often in different databases, so maintaining strict ACID is challenging. 
Instead, we use patterns like eventual consistency, sagas, or outbox patterns to manage distributed transactions safely.

* **Atomicity** - All steps in a transaction succeed or none do
* **Consistency** - After a transaction, the data must follow all rules and constraints.
* **Isolation** - Running multiple transactions at the same time shouldn’t affect each other.
* **Durability** - Once a transaction is committed, it won’t be lost — even if the system crashes.

Why?
Because:
* Services may run on different servers
* Databases are separate
* Network failures can happen between services
* Distributed transactions are complex and slow

# 5. **What is REST and API**
An API is a set of rules that allows different software applications or components to communicate.

It’s like a contract:
“If you send me this kind of input, I’ll give you this kind of output.”

- A REST API is a type of API built using the HTTP protocol, where data is treated as resources and identified by URLs.

It follows six REST principles like:
* Statelessness
* Client-server separation
* Uniform interface
* Use of HTTP methods (GET, POST, PUT, DELETE)

**Example to Compare API vs REST API**
* A General API (non-REST) — Example: Windows File API (Local/OS API)
```HANDLE hFile = CreateFile("example.txt", GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);```
This is a system-level API in Windows. It allows programs to create or open files — but it doesn’t use HTTP or work over the web.

*  A REST API Example — Web API to get user data:
```GET https://api.example.com/users/123```
 
Server Response : Json
```
{
  "id": 123,
  "name": "Alice",
  "email": "alice@example.com"
}
```

This is a REST API call to get user info for user with ID 123.
* You’re using the HTTP GET method
* /users/123 identifies the resource
* The server returns structured data (JSON)

# **What is Nginx?**
Nginx (pronounced “engine-x”) is a high-performance web server that can also act as:
* A reverse proxy
* A load balancer
* An API gateway
* A static file server

It is widely used to serve websites and APIs because it’s fast, lightweight, and scalable.

### **What is the role of Nginx?**
1. Web Server
* Serves static files like HTML, CSS, JavaScript, and images directly to the client.

2. Reverse Proxy
* Forwards requests to another server (like a Node.js or .NET app), and sends back the response to the client.
- Used to hide backend services from direct exposure and secure them.

3. Load Balancer
* Distributes incoming traffic across multiple backend servers for better performance and reliability.
- Instances of your app → Nginx balances load between them

4. SSL Termination
- Handles HTTPS/SSL connections at the edge, so your backend can just deal with HTTP.

5. API Gateway / Routing
- Routes different API endpoints to different backend services (very useful in microservices).