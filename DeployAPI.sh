dotnet publish FoodDeliveryApp.API -c Release

az webapp deployment source config-zip `
  --resource-group rg-fooddelivery `
  --name api-fooddelivery-<random> `
  --src FoodDeliveryApp.API/bin/Release/net9.0/publish.zip