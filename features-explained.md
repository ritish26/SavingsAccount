## **Features**

## 1. **Converted long-running sync commands into asynchronous background jobs, resolving timeout issues and boosting system reliability.**
I worked on a feature where users were trying to generate a report, but the report wasn’t being generated properly.
The issue was that the report generation logic was written directly in the controller or command handler, and it was trying to do
everything synchronously. For large reports, this caused performance issues — especially because it involved loading heavy data like reports download with large data.
This led to high resource usage and timeouts, and the UI showed a loading screen for a long time, giving a poor user experience.

To solve this, I refactored the logic into a background job using Hangfire. I created a separate service to handle the report generation,
queued it through Hangfire, and exposed an API endpoint so the frontend could check whether the report was ready. Once ready, 
the user could download it. This made the system more responsive and scalable.

1. Why did you choose Hangfire? What are its advantages?
   I chose Hangfire because it integrates easily with .NET applications, supports background processing without requiring an external message queue, and provides a built-in dashboard for monitoring jobs.
   It also supports retry policies, scheduled jobs, and persistent storage using SQL Server or Redis — which made it a good fit for our needs.

2. What happens if the Hangfire job fails? Do you have retries or error handling in place?
Yes, Hangfire automatically retries failed jobs by default (typically 10 times).
   In our case, we also:
- Logged the exception
- Monitored failure via the Hangfire dashboard
- Notified the admin in case of consistent failures 
- You can configure custom retry logic or move jobs to a dead-letter queue if needed.

3. How do you notify the user once the report is ready?
We implemented a polling approach from the frontend.
After queuing the job, we return a reportId and expose an API like /report/status/{reportId}.
The frontend periodically checks this endpoint. Once the report is ready, we show a “Download” button.

4. Where is the report stored after generation?
We stored the report temporarily in cloud storage (or local disk, depending on your setup), with a link valid for a limited time.
Each report is associated with a unique token or user-specific ID to ensure only the right user can access it.

5. What if multiple users request the same report? Do you cache results or regenerate?
We check if a similar report already exists (based on filters or user context).
If so, we return the existing one instead of regenerating it.
This helps optimize performance and reduce processing load.

Multiple Workers for Parallel Processing

Hangfire uses background workers to process jobs.
By default, each worker processes one job at a time.
•	You can increase the number of workers per server to process multiple jobs in parallel.
•	For example: 5 workers = 5 reports processed at the same time.


## 2. **Led the development of an SFTP-based HRMS microservice from scratch, using Hangfire for scheduled jobs to consume HRMS data daily, reducing manual effort by 40%.**
I worked on automating the employee data processing workflow for clients by integrating an SFTP client with our QapMap product.

Clients would drop an Excel sheet onto a shared SFTP folder. Previously, the data had to be downloaded and updated manually in our system.
I developed an automated service that:
•	Connects to the SFTP server
•	Periodically scans for new files
•	Downloads the Excel file
•	Parses the employee records
•	Updates or deletes records in the database accordingly

This reduced manual work by over 95% and ensured data was updated securely and consistently without human error

- What Was Your Solution?
1. Used an SFTP client to connect to a shared folder
2. Automated job that polls, processes, and updates the data
3. Integrated Excel parsing (e.g., using EPPlus or ClosedXML in .NET)
4. Logged results and errors for auditability

1. Why did you choose SFTP over other file transfer methods?
   SFTP (Secure File Transfer Protocol) is encrypted, secure, and widely used in enterprise environments.
   Many of our clients already had infrastructure for SFTP, so it was the most practical and secure way to receive files.

2. How did you handle file parsing and validation?
I used a library like EPPlus or ClosedXML to read Excel files.
I implemented:
* Schema validation (required columns, correct formats)
* Business rules (e.g., email must be valid, status must be active/inactive)
* Error logging for each invalid row
* Valid records were processed, and invalid ones were reported to the client or logged.
  
3. How frequently does your system poll the SFTP server?
We scheduled it using a background service (e.g., Hangfire or Quartz) to poll every few minutes.
Polling frequency was configurable based on client needs.

**Recommended Library: SSH.NET (Renci.SshNet)**

## **Enhanced and maintained an internal migration tool that facilitated seamless client transitions, handling 20+ enterprise migrations.**
At Qapita, we migrated data from an older system that stored stakeholder details, grants, exercises, and equity-related records in SQL 
using a traditional relational model. In the new product — QapMap — we adopted an event-sourced model with CQRS, where data is represented
as a stream of domain events.
We had to not only transform the data structure but also design new domain models and replayable events that faithfully represented the
historical states of each stakeholder’s equity position. This involved building custom migration scripts that extracted SQL data, mapped 
them to our new domain models, and published corresponding events like GrantCreated, StakeholderAdded, or ExerciseRequested.
This ensured the new system could replay the full lifecycle of equity transactions and be audit-compliant, while supporting scalability and modern architecture.

**Steps Involved**
Extract (Read) Data from SQL
* Use an ETL tool or custom migration service to connect to the SQL database.
* Pull records from the relevant tables (e.g., Stakeholders, Grants, Exercises).
* Each row represents a snapshot of state, not a change over time.

Transform to Domain Model
* Convert the flat SQL records into rich domain objects (aggregates).
* These aggregates represent entities in the new system, e.g., EquityGrant, Stakeholder, ExerciseRequest.
   