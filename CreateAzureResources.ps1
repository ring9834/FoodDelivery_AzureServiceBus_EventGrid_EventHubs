# Azure Resources Setup Script
# Run this PowerShell script to create all required Azure resources

# Variables - Update these with your values
$resourceGroupName = "rg-fooddelivery"
$location = "eastus"
$cosmosAccountName = "cosmos-fooddelivery-$(Get-Random)"
$serviceBusNamespace = "sb-fooddelivery-$(Get-Random)"
$eventGridTopicName = "eg-fooddelivery"
$eventHubNamespace = "eh-fooddelivery-$(Get-Random)"
$eventHubName = "location-telemetry"
$redisName = "redis-fooddelivery-$(Get-Random)"
$signalRName = "signalr-fooddelivery-$(Get-Random)"
$notificationHubNamespace = "nh-fooddelivery-$(Get-Random)"
$notificationHubName = "food-delivery-notifications"
$appInsightsName = "ai-fooddelivery"
$storageAccountName = "stfooddelivery$(Get-Random)"
$functionAppName = "func-fooddelivery-$(Get-Random)"
$apiAppName = "api-fooddelivery-$(Get-Random)"
$signalRHubAppName = "signalrhub-fooddelivery-$(Get-Random)"

# Login to Azure
Write-Host "Logging in to Azure..." -ForegroundColor Green
az login

# Create Resource Group
Write-Host "Creating Resource Group..." -ForegroundColor Green
az group create --name $resourceGroupName --location $location

# Create Cosmos DB Account
Write-Host "Creating Cosmos DB Account..." -ForegroundColor Green
az cosmosdb create `
    --name $cosmosAccountName `
    --resource-group $resourceGroupName `
    --default-consistency-level Session `
    --locations regionName=$location failoverPriority=0 isZoneRedundant=False

# Create Cosmos DB Database and Containers
Write-Host "Creating Cosmos DB Database and Containers..." -ForegroundColor Green
az cosmosdb sql database create `
    --account-name $cosmosAccountName `
    --resource-group $resourceGroupName `
    --name FoodDeliveryDb

az cosmosdb sql container create `
    --account-name $cosmosAccountName `
    --database-name FoodDeliveryDb `
    --name Orders `
    --partition-key-path "/customerId" `
    --resource-group $resourceGroupName

az cosmosdb sql container create `
    --account-name $cosmosAccountName `
    --database-name FoodDeliveryDb `
    --name Vendors `
    --partition-key-path "/id" `
    --resource-group $resourceGroupName

az cosmosdb sql container create `
    --account-name $cosmosAccountName `
    --database-name FoodDeliveryDb `
    --name DeliveryPersons `
    --partition-key-path "/id" `
    --resource-group $resourceGroupName

az cosmosdb sql container create `
    --account-name $cosmosAccountName `
    --database-name FoodDeliveryDb `
    --name Customers `
    --partition-key-path "/id" `
    --resource-group $resourceGroupName

# Create Service Bus Namespace and Topic
Write-Host "Creating Service Bus..." -ForegroundColor Green
az servicebus namespace create `
    --resource-group $resourceGroupName `
    --name $serviceBusNamespace `
    --location $location `
    --sku Standard

az servicebus topic create `
    --resource-group $resourceGroupName `
    --namespace-name $serviceBusNamespace `
    --name order-assignment

az servicebus queue create `
    --resource-group $resourceGroupName `
    --namespace-name $serviceBusNamespace `
    --name notifications

# Create Event Grid Topic
Write-Host "Creating Event Grid Topic..." -ForegroundColor Green
az eventgrid topic create `
    --name $eventGridTopicName `
    --location $location `
    --resource-group $resourceGroupName

# Create Event Hub Namespace and Event Hub
Write-Host "Creating Event Hub..." -ForegroundColor Green
az eventhubs namespace create `
    --resource-group $resourceGroupName `
    --name $eventHubNamespace `
    --location $location `
    --sku Standard

az eventhubs eventhub create `
    --resource-group $resourceGroupName `
    --namespace-name $eventHubNamespace `
    --name $eventHubName `
    --message-retention 1 `
    --partition-count 4

# Create Azure Cache for Redis
Write-Host "Creating Redis Cache..." -ForegroundColor Green
az redis create `
    --location $location `
    --name $redisName `
    --resource-group $resourceGroupName `
    --sku Basic `
    --vm-size c0

# Create SignalR Service
Write-Host "Creating SignalR Service..." -ForegroundColor Green
az signalr create `
    --name $signalRName `
    --resource-group $resourceGroupName `
    --location $location `
    --sku Standard_S1 `
    --service-mode Default

# Create Notification Hub Namespace and Hub
Write-Host "Creating Notification Hub..." -ForegroundColor Green
az notification-hub namespace create `
    --resource-group $resourceGroupName `
    --name $notificationHubNamespace `
    --location $location `
    --sku Standard

az notification-hub create `
    --resource-group $resourceGroupName `
    --namespace-name $notificationHubNamespace `
    --name $notificationHubName

# Create Application Insights
Write-Host "Creating Application Insights..." -ForegroundColor Green
az monitor app-insights component create `
    --app $appInsightsName `
    --location $location `
    --resource-group $resourceGroupName

# Create Storage Account for Functions
Write-Host "Creating Storage Account..." -ForegroundColor Green
az storage account create `
    --name $storageAccountName `
    --location $location `
    --resource-group $resourceGroupName `
    --sku Standard_LRS

# Create App Service Plan
Write-Host "Creating App Service Plan..." -ForegroundColor Green
az appservice plan create `
    --name "asp-fooddelivery" `
    --resource-group $resourceGroupName `
    --location $location `
    --sku B1 `
    --is-linux

# Create Function App
Write-Host "Creating Function App..." -ForegroundColor Green
az functionapp create `
    --resource-group $resourceGroupName `
    --consumption-plan-location $location `
    --runtime dotnet-isolated `
    --functions-version 4 `
    --name $functionAppName `
    --storage-account $storageAccountName

# Create API Web App
Write-Host "Creating API Web App..." -ForegroundColor Green
az webapp create `
    --resource-group $resourceGroupName `
    --plan "asp-fooddelivery" `
    --name $apiAppName `
    --runtime "DOTNETCORE:8.0"

# Create SignalR Hub Web App
Write-Host "Creating SignalR Hub Web App..." -ForegroundColor Green
az webapp create `
    --resource-group $resourceGroupName `
    --plan "asp-fooddelivery" `
    --name $signalRHubAppName `
    --runtime "DOTNETCORE:8.0"

# Get Connection Strings
Write-Host "`nRetrieving Connection Strings..." -ForegroundColor Green

$cosmosConnectionString = az cosmosdb keys list `
    --name $cosmosAccountName `
    --resource-group $resourceGroupName `
    --type connection-strings `
    --query "connectionStrings[0].connectionString" `
    --output tsv

$serviceBusConnectionString = az servicebus namespace authorization-rule keys list `
    --resource-group $resourceGroupName `
    --namespace-name $serviceBusNamespace `
    --name RootManageSharedAccessKey `
    --query primaryConnectionString `
    --output tsv

$eventGridKey = az eventgrid topic key list `
    --name $eventGridTopicName `
    --resource-group $resourceGroupName `
    --query key1 `
    --output tsv

$eventGridEndpoint = az eventgrid topic show `
    --name $eventGridTopicName `
    --resource-group $resourceGroupName `
    --query endpoint `
    --output tsv

$eventHubConnectionString = az eventhubs namespace authorization-rule keys list `
    --resource-group $resourceGroupName `
    --namespace-name $eventHubNamespace `
    --name RootManageSharedAccessKey `
    --query primaryConnectionString `
    --output tsv

$redisConnectionString = az redis list-keys `
    --name $redisName `
    --resource-group $resourceGroupName `
    --query primaryKey `
    --output tsv

$redisHostName = az redis show `
    --name $redisName `
    --resource-group $resourceGroupName `
    --query hostName `
    --output tsv

$signalRConnectionString = az signalr key list `
    --name $signalRName `
    --resource-group $resourceGroupName `
    --query primaryConnectionString `
    --output tsv

$notificationHubConnectionString = az notification-hub authorization-rule list-keys `
    --resource-group $resourceGroupName `
    --namespace-name $notificationHubNamespace `
    --notification-hub-name $notificationHubName `
    --name DefaultFullSharedAccessSignature `
    --query primaryConnectionString `
    --output tsv

$appInsightsConnectionString = az monitor app-insights component show `
    --app $appInsightsName `
    --resource-group $resourceGroupName `
    --query connectionString `
    --output tsv

# Output Configuration
Write-Host "`n=== CONFIGURATION VALUES ===" -ForegroundColor Cyan
Write-Host "Copy these values to your appsettings.json and local.settings.json files" -ForegroundColor Yellow
Write-Host ""
Write-Host "CosmosDb:ConnectionString: $cosmosConnectionString" -ForegroundColor White
Write-Host "ServiceBus:ConnectionString: $serviceBusConnectionString" -ForegroundColor White
Write-Host "EventGrid:TopicEndpoint: $eventGridEndpoint" -ForegroundColor White
Write-Host "EventGrid:AccessKey: $eventGridKey" -ForegroundColor White
Write-Host "EventHub:ConnectionString: $eventHubConnectionString" -ForegroundColor White
Write-Host "Redis:ConnectionString: ${redisHostName}:6380,password=${redisConnectionString},ssl=True,abortConnect=False" -ForegroundColor White
Write-Host "SignalR:ConnectionString: $signalRConnectionString" -ForegroundColor White
Write-Host "NotificationHub:ConnectionString: $notificationHubConnectionString" -ForegroundColor White
Write-Host "ApplicationInsights:ConnectionString: $appInsightsConnectionString" -ForegroundColor White
Write-Host ""
Write-Host "=== DEPLOYMENT ENDPOINTS ===" -ForegroundColor Cyan
Write-Host "Function App: https://${functionAppName}.azurewebsites.net" -ForegroundColor White
Write-Host "API App: https://${apiAppName}.azurewebsites.net" -ForegroundColor White
Write-Host "SignalR Hub App: https://${signalRHubAppName}.azurewebsites.net" -ForegroundColor White
Write-Host ""
Write-Host "Setup completed successfully!" -ForegroundColor Green