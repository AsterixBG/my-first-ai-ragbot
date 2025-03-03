# AI Chatbot with Retrieval-Augmented Generation (RAG) for .NET

A beginner-friendly showcase for building AI-powered agents in the .NET ecosystem.  
This project provides a simple console-based chatbot utilizing Microsoft Semantic Kernel, Ollama, Qdrant for memory storage and Weather plugin that allows it to go to [OpenWeatherMap](https://openweathermap.org) and check the current weather.

Spicing it up a little with Retrieval-Augmented Generation (RAG)!  
This is our friend from the [my first ai chatbot repository](https://github.com/AsterixBG/my-first-ai-chatbot).  
He has grown a bit smarter and now knows how to interact with our world.  
More specifically, he can check the weather in a given city when asked.  
He goes out, retrieves the data for you, and lets you know.  

In my previous example, the LLM used a predefined prompt that made him aware of the conversation's context.  
Now, he has the capacity to take action when the user's request can be accomplished using a tool.

### Key Technical Aspects

✅ In-memory caching for city geolocation within the application's lifecycle. We query the database only the first time during the bot's runtime.  
✅ Database persistence for geolocations of queried cities since that data won't ever change (hopefully).  
✅ API calls to a third-party service to fetch real-time weather data.

### Why This Approach?

In my experience, getting to know the instruments in depth first always produces the best results.  
Many times, when skipping the fundamentals and going "live", I was blindsided by the intricacies of a new technology I had jumped into without proper groundwork.  
There were even cases where I ended up reinventing the wheel.  

That's why, in this mini-series, I decided to take the console application approach.  
The goal is clear: to showcase that exploring new concepts can be done in steps - with managed complexity and minimal overhead.

I firmly believe you don't need to research every possible framework or drown yourself in endless documentation before taking action.
It's easier to start small, build momentum at the right pace, and expand from there.  
The alternative? Go big and risk getting lost in an ocean of information.

This is the evolutionary result of our efforts to get things done.  
Sure, we're adding complexity, but one can always start small and gain traction and velocity along the way.  

## Prerequisites

Before running this project, make sure you have the following tools installed:

| Tool | Version | Download Link |
|------|---------|--------------|
| **.NET SDK** | 8.0 | [Download .NET 8](https://dotnet.microsoft.com/en-us/download) |
| **Ollama** | 0.5.10 (latest) | [Download Ollama](https://ollama.com/download) |
| **Docker Desktop** | Latest | [Download Docker Desktop](https://www.docker.com/products/docker-desktop/) |
| **Qdrant** | 1.13.3 (latest) | [Qdrant Dashboard](http://localhost:6333/dashboard) |
| **Microsoft Visual Studio Community** | 2022 (64-bit) - Version 17.13.0 | [Download Visual Studio Community](https://visualstudio.microsoft.com/vs/community/) |
| **WeatherMap API Key** | X | [Sign Up](https://home.openweathermap.org/users/sign_up) |

## Installation  

1. Clone this repository:

```sh
   git clone https://github.com/AsterixBG/my-first-ai-ragbot.git
```

Navigate to the project directory:
```sh
cd my-first-ai-ragbot/src
```

Restore dependencies:

```sh
dotnet restore
```

Run the application:

```sh
dotnet run
```

## How to Use the AI Chatbot

Follow the steps below to run and interact with the agent.

### 1 Start the Required Services

Before running the chatbot, make sure you have the necessary services up and running:

- **Ollama** (local LLM model)  
  Start Ollama if it's not running:  

```sh
ollama run mistral
```

- **Qdrant** (vector memory storage)

```sh
docker run -d --name qdrant -p 6333:6333 qdrant/qdrant
```

### 2 Run the Chatbot

Once the dependencies are running, navigate to the `src` folder and start the console application:

```sh
cd src
dotnet run
```

### 3 Interact with the AI Chatbot

Once running, the console will prompt you to enter a message:

```
AI Chatbot (RAG) – Type 'exit' to quit.
Enter your question:
```

Simply type your message and press `Enter`. The chatbot will analyze previous conversations stored in **Qdrant** and generate a response using the **Mistral** model.

Example interaction:

```
Enter your question: What is the weather in the capital of France?
[DBUG] Checking memory for Paris
[DBUG] Getting Paris from API.
[DBUG] Got {"Lat":48.8588897,"Lon":2.3200410217200766} from API.
AI:  The current weather in Paris, France is 4.21°C with clear skies.
```

### 4 Exiting the Chatbot

To exit, simply type `exit`

