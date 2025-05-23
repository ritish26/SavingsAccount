✅ Project Description

I am currently working on a project in QapMap, a platform used to manage cap tables—documents that outline a company’s ownership structure, listing all shareholders and their respective ownership percentages.
We assist startups by providing a detailed view of their ESOPs and equity. Stakeholders can include both investors and employees.
The platform supports features like:
1. Share buybacks
2. Vesting
3. render
3. Exercise of equity awards

⸻

✅ Tech Stack and One-Liner Benefits
1. C# / .NET: A modern, high-performance framework for building scalable, maintainable backend services.
2. Event Store: Enables reliable event sourcing by storing the full history of state changes for auditability and traceability.
3. MongoDB: A flexible NoSQL database ideal for handling dynamic and hierarchical data structures like cap tables.
4. AutoMapper: Simplifies object-to-object mapping, reducing boilerplate code in DTO conversions.
5. CQRS Pattern: Separates read and write logic for better performance, scalability, and maintainability.
6. Docker: Provides lightweight containers for consistent development, testing, and deployment across environments.
7. NServiceBus: Handles asynchronous messaging and distributed workflows for resilient and decoupled microservices.

⸻

✅ Event Store vs MongoDB

We use Event Store for writes and MongoDB for reads to implement the CQRS pattern.

Benefits:
1. Event Store captures every change as an immutable event for auditability and traceability.
2. MongoDB is optimized for fast, flexible reads.
3. Improves overall system performance by decoupling read and write operations.
4. Enables independent scaling of reads and writes.
5. Increases system resilience and maintainability.


✅ Why do you want to switch jobs? (Answer for 5 years of experience, with last company being the only one)
“I’ve had a great learning journey in my current company, where I’ve grown both technically and professionally. 
Over the last few years, I’ve worked on complex systems using technologies like .NET, Event Sourcing, CQRS, and distributed 
architecture, and I’m proud of the impact I’ve made.
However, I feel ready for a new challenge—something that pushes me further, exposes me to different problem spaces, and gives me the
opportunity to grow in areas like system design, architecture decisions, or even leadership.
I’m looking for an environment where I can continue to learn, work with smart people, and contribute to meaningful projects at scale.”

✅ CI/CD Deployment Process (Azure + AWS Hybrid)

Our organization follows a hybrid CI/CD pipeline that integrates both Azure for source control and AWS for build and deployment. Below are the detailed steps:
1. Source Code Management
   The source code is managed in Azure Repos.
   Developers create Pull Requests (PRs) to the main branch.

2. Pipeline Trigger
   Every pull request or commit to the main branch triggers an Azure DevOps pipeline.
   This pipeline initiates the build and deployment workflow.

3. Build Automation using AWS CodeBuild
   The pipeline delegates the build job to AWS CodeBuild.
   CodeBuild performs the following:
     - Compiles the application
     - Runs unit tests and code analysis (if configured)
     - Builds Docker images

4. Artifact & Image Storage
   Upon successful build:
   Build artifacts (e.g., binaries, compiled files) are stored in Amazon S3.
   Docker images are pushed to Amazon Elastic Container Registry (ECR) with tag (myapp:latest)

5. Deployment via Amazon EKS (Kubernetes)
   The latest Docker image is pulled from Amazon ECR.
   A rolling deployment is initiated on Amazon EKS (Kubernetes):
   Kubernetes creates new pods(pod contain one or more container) with the updated image.
   Once new pods are ready, it gradually terminates the old pods.
   This ensures zero downtime and smooth updates.