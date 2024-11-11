# Financial Instrument Price Service

This project provides REST API endpoints and a WebSocket endpoint for accessing and subscribing to live financial instrument prices. The service leverages data from the public data provider Tiingo and is implemented using .NET.

## Prerequisites

- Visual Studio Code (VS Code)
- .NET SDK (version 8.0 or higher)
- Postman for API testing
- WebSocket testing client (e.g., Postman or any WebSocket client)

## Setting Up the Project

### 1. Clone the Repository

https://github.com/Adil-Farhan/AmegaTechAssessment.git

Clone the repository to your local machine:

## Building and Running the Project

### 1. Steps to Build and Run in VS Code

Open the project folder in VS Code.
Ensure that the necessary .NET extensions are installed (e.g., C# for Visual Studio Code).
Open a terminal in VS Code and navigate to the root of your project.
Run the following command to build the project:
To build the project "dotnet build".
To run the project "dotnet run".
The server will start and listen at http://localhost:5031/.

### 2. Testing REST API Endpoints

A Postman collection is provided in the webapp folder for testing the REST APIs. It includes the following key endpoints:

GetFinancialInstrumentList:
Endpoint: GET http://localhost:5031/api/FinancialInstrument/GetFinancialInstrument
Retrieves a list of available financial instruments.
For Example
{
"code": 200,
"message": "Financial Instruments Fetched Successfully.",
"data": [
{
"name": null,
"ticker": "quick2usdc"
},
{
"name": "BIGTIME-USDT (BIGTIME/USDT)",
"ticker": "bigtimeusdt"
},
{
"name": "CTSI CTSIUSD",
"ticker": "ctsiusd"
},
{
"name": "OXY (OXY/USD)",
"ticker": "oxyusd"
},
{
"name": "RENDER (RENDER/EUR)",
"ticker": "rendereur"
},
{
"name": "Ethereum (ETH/BTC)",
"ticker": "ethbtc"
}
]
}
Get Price:
Endpoint: GET http://localhost:5031/api/FinancialInstrument/GetPrices?instrument=btcusdt
Retrieves the current price of a specified financial instrument(e.g., btcusdt).
For Example

{
"code": 200,
"message": "Financial Instrument btcusdt Price Fetched Successfully.",
"data": {
"open": 80887.64,
"close": 80916.18,
"high": 80925.4,
"low": 80826.555,
"volume": 202.49965,
"date": "2024-11-11T12:35:00+05:00"
}
}

### 3. Creating a WebSocket Request in Postman
For creating a websocket I am attaching a video in email because I was not able to export that postman collection.
Open Postman and navigate to the WebSocket request interface.
Enter the WebSocket URL: ws://localhost:5031/ws.
Connect to the WebSocket.
Send the instrument ticker (e.g., btcusdt) to start receiving subscription data.
The WebSocket will connect, and you will begin receiving real-time updates for the specified instrument every minute.
