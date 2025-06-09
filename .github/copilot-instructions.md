
This is an MVP (Minimum Viable Product) application using .NET 9 Blazor Server framework. Blazor server does not use web api instead the client uses SignalR. The application will be built using C# and will follow best practices for clean arcitecture and microsoft coding standards. If possible code a strcture to make allow for easy refactor to use blazor auto interactive mode which uses both server and webassembly (with web api).

Always create unit tests for critical functions. Write comments to explain complex logic or important decisions. The code should be modular, allowing for easy updates and maintenance. Use dependency injection to manage service lifetimes and dependencies effectively. Only define interfaces for the service that ordinarly have more than one implementation. Use meaningful variable and function names. Use error handling to manage exceptions gracefully.

Folder structure should be as follows:
Client - this is main folder for the code for the web client
client.Common - this is the common code folder for the client. This code is common to all applications built using this project as a templte - this includes shared models, services, utilities, componets, and layouts that are not specific to any one service. This is like Core in Clean Architecture.
client.Feature.Base - this is the "base" feature folder for the client. These feature are common to all applications
Client.Feature.<FeatureName> - this is the feature folder for the client. These feature are specific to the application. This sould containt sub folders for Interfaces, Services, Models, componets, and layouts and Utilities. The Interfaces folder should contain the interfaces for the services in this feature. The Services folder should contain the implementation of the services in this feature. The Models folder should contain the models for this feature. The Utilities folder should contain any utilities for this feature.
Server - this is main folder for the code for the server
Server.Common - this is the common code folder for the server. his code is common to all applications built using this project as a templte - this includes shared models, services, and utilities that are not specific to any one service. This is like Core in Clean Architecture.
Server.Feature.Base - this is the "base" feature folder for the server. These feature are common to all applications
Server.Feature.<FeatureName> - this is the feature folder for the server. These feature are specific to the application. This sould containt sub folders for Interfaces, Services, Models, and Utilities. The Interfaces folder should contain the interfaces for the services in this feature. The Services folder should contain the implementation of the services in this feature. The Models folder should contain the models for this feature. The Utilities folder should contain any utilities for this feature.

Settings should be stored in the database with correspond User interface. If setting is not in database then it should be in the appsettings.json file. The settings should be loaded into a service that can be injected into the components that need them. The settings should be strongly typed and use the Options pattern.



