dotnet publish FoodDeliveryApp.SignalRHub -c Release

az webapp deployment source config-zip `
  --resource-group rg-fooddelivery `
  --name signalrhub-fooddelivery-<random> `
  --src FoodDeliveryApp.SignalRHub/bin/Release/net9.0/publish.zip