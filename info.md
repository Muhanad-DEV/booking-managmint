
SULTAN QABOOS UNIVERSITY
COLLEGE OF SCIENCE
DEPARTMENT OF COMPUTER SCIENCE



Course Name: COMP4701 – Web application
University: Sultan Qaboos University
Semester: Fall25
Instructor: Dr.Abdullah
Project Title: Event booking system
Team Members:
1. Muhannad Al Muwaiti  (ID: 138794)
2. Yaqoob Albaluchi (ID: 138590 )
3.Ahmed Alabri (ID:138690  )




























Project Information
Introduction
An Event Booking System is a digital platform that enables users to browse, book, and manage event reservations efficiently. It allows organizers to create events with details such as date, time, location, and capacity, while attendees can register, purchase tickets, or reserve seats online. The system supports secure payments, automated confirmations, and notifications. It may also provide features like seat selection, cancellation, and attendance tracking. By streamlining the booking process and reducing manual errors, the system improves user experience, increases event participation, and helps organizers manage events more effectively.

Objectives
To design and develop an online platform for browsing, booking, and managing event reservations.
To enable event organizers to create and manage event listings with essential details (date, time, venue, capacity, ticket type, ...).
To allow users to easily register for events, purchase tickets securely, and receive automated confirmations and reminders.
To provide a reliable and efficient system that minimizes manual work, reduces booking errors, and enhances the user experience.
To support additional functionalities such as seat selection, event cancellation, and attendance tracking.
Motivation and Reason for Selection
This project was chosen because managing events is a common need in universities and organizations, yet many still use slow and manual registration methods. An Event Booking System makes the process faster, easier, and more organized by allowing users to book, pay, and receive updates online. With today’s move toward digital services and contactless solutions, especially after COVID-19, this system is both practical and necessary. It also provides a great opportunity to apply software development skills such as database design, authentication, and system management to solve a real-world problem.



Real-World Relevance and Benefits

For Organizers:
Simplifies event creation and management.
Reduces administrative workload and human error.
Provides real-time statistics and attendance tracking.

For Attendees:
Offers an easy and convenient booking experience.
Provides instant confirmations and notifications.
Ensures secure and transparent payment processing.

For Institutions/Communities:
Encourages greater participation in events.
less paper use.


Problem and Solution
Many organizations still use manual or partly digital methods to manage event bookings, which often causes double bookings, delays, and poor communication between organizers and participants. To solve these issues, the Event Booking System offers an online platform that automates the whole process from creating events and selling tickets to sending confirmations and reminders. It keeps data accurate, updates information in real time, and ensures secure payments. This system will benefit event organizers, students, employees, and the general public by saving time, reducing errors, and making event management easier and more efficient.



User Interviews
customer
The first interview was conducted with Ayoob Alabri, who regularly participates in university events and workshops. The student explained that the current registration process is often confusing and time-consuming since most events use different Google Forms or manual sign-up sheets. They suggested having a single online platform where all upcoming events are listed with clear details such as date, time, and location. The student also mentioned the importance of receiving instant confirmations after booking and reminders before the event. Additionally, they would like to have the option to cancel or change their booking easily without contacting the organizer directly.
Administrator
The second interview was held with Mr.Abdullah Albulochi, responsible for managing the event. Mr.Abdullah shared that handling registrations manually using spreadsheets often leads to errors, such as duplicate entries and overbooking. They emphasized the need for an automated system that can limit seat availability, generate attendance reports, and allow organizers to manage events from a dashboard. They also suggested integrating a QR code check-in feature to simplify attendance tracking during events. Overall, the interview highlighted the need for an efficient, centralized, and user-friendly system that reduces manual work and improves event management.

Requirements
Functional Requirements
1.User Accounts and Authentication
The system shall allow users to register, log in, log out, and reset passwords. User sessions will be managed securely to protect personal data.
2.Role Management
The system shall support two main roles: Attendee and Organizer,Admin. Only organizers will have permissions to create and manage events.
3.Browse Events
Attendees shall be able to view a paginated and searchable list of events with filtering options such as date, category, and location.
4.Event Details Page
Each event page shall display complete information including the title, description, date and time, venue, capacity, remaining seats, ticket types, and price.
5.Ticket and Seat Selection
Users shall be able to select tickets or seats. The system will temporarily lock selected seats to prevent double bookings.
6.Ticket Issuance
the system shall automatically issue digital tickets with unique code, and update event capacity.
7.Notifications and Reminders
Users shall receive automated email confirmations after booking, along with event reminders and cancellation notices.
8.Cancellations 
Users shall be able to cancel their bookings within organizer defined rules. The system will update capacity and ticket status accordingly.
9.User Dashboard
Each user shall have a personal dashboard to view upcoming events, download tickets, check order history, and manage cancellations.
10.Organizer Dashboard
Organizers shall have access to an administrative dashboard to create, edit, and publish events, manage capacity, upload images, and track performance.
11.Attendance and Check-In
Staff shall verify attendee tickets by scanning user codes at the venue to mark attendance and prevent duplicate entries.
12.Data Validation
All system forms shall validate required inputs such as email format, event dates, and ticket quantities before submission.
Non-Functional Requirements
1.Ease of Use
The website should be simple and easy for anyone to use. Users must be able to find events, book tickets, and make payments without needing help. First-time users should finish the booking process in about three minutes or less.
2.Fast Performance
The system should work quickly. The homepage and events list should load in less than three seconds, and actions like viewing events or booking tickets should respond in under one second.
3.Reliability
The website should always be available when users need it, especially during event booking periods. It should stay online at least 99% of the time and work smoothly without crashing or losing data.
4.Accurate Data
All information stored in the system should be correct and up to date.
5.Security and Privacy
The system must keep all user information safe. Passwords should be encrypted, and all connections should be secure.
6.Scalability
The system should work well even if many people use it at the same time.
7.Easy Maintenance
The system should be well organized so developers can easily fix errors or add new features.


