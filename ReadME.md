# DiscountGenerator

## Overview
DiscountGenerator is a solution that generates discount codes and simulates the code usage as well. It consists of a server-side application built with C#, .NET 8 with SignalR as communication protocol, and a client-side application built with JavaScript and React. The solution is designed to generate and manage discounts.

## Prerequisites
- .NET 8 SDK
- Node.js and npm
- Docker and Docker Compose (for running the solution in containers)

## Project Structure
- `DiscountGenerator/`: Contains the server-side application.
- `discount-generator.client/`: Contains the client-side application.
- `docker-compose.yml`: Docker Compose configuration file to run the entire solution.

## Getting Started

### Clone the Repository
```sh
git clone https://github.com/ifeanyilawrence/discount-generator.git
cd DiscountGenerator
```

### Build and Run with Docker Compose
1. Ensure Docker and Docker Compose are installed and running.
2. Navigate to the root directory of the project.
3. Run the following command to build and start the services:
```sh
docker-compose up --build
```
4. The server will be available at http://localhost:7083 and the client at http://localhost:3000.

### Running the server locally
1. Navigate to the `DiscountGenerator` directory.
2. Restore the dependencies and run the server:
```sh
dotnet restore
dotnet run
```
3. The server will be available at http://localhost:7083.

### Running the client locally
1. Navigate to the `discount-generator.client` directory.
2. Install the dependencies and run the client:
```sh
npm install
npm start
```
3. The client will be available at http://localhost:3000.

## Assumption
- Even used codes cannot be repeated

## Contact
If you have any questions or feedback, feel free to reach out to me at