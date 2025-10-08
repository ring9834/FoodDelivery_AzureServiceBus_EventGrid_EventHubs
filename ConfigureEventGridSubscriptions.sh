# Subscribe to OrderCreated events
az eventgrid event-subscription create `
  --name order-created-subscription `
  --source-resource-id /subscriptions/<subscription-id>/resourceGroups/rg-fooddelivery/providers/Microsoft.EventGrid/topics/eg-fooddelivery `
  --endpoint-type azurefunction `
  --endpoint /subscriptions/<subscription-id>/resourceGroups/rg-fooddelivery/providers/Microsoft.Web/sites/func-fooddelivery-<random>/functions/OrderCreatedFunction `
  --included-event-types OrderCreated

# Subscribe to OrderUpdated events
az eventgrid event-subscription create `
  --name order-updated-subscription `
  --source-resource-id /subscriptions/<subscription-id>/resourceGroups/rg-fooddelivery/providers/Microsoft.EventGrid/topics/eg-fooddelivery `
  --endpoint-type azurefunction `
  --endpoint /subscriptions/<subscription-id>/resourceGroups/rg-fooddelivery/providers/Microsoft.Web/sites/func-fooddelivery-<random>/functions/OrderUpdatedFunction `
  --included-event-types OrderUpdated

# Subscribe to OrderDispatched events
az eventgrid event-subscription create `
  --name order-dispatched-subscription `
  --source-resource-id /subscriptions/<subscription-id>/resourceGroups/rg-fooddelivery/providers/Microsoft.EventGrid/topics/eg-fooddelivery `
  --endpoint-type azurefunction `
  --endpoint /subscriptions/<subscription-id>/resourceGroups/rg-fooddelivery/providers/Microsoft.Web/sites/func-fooddelivery-<random>/functions/OrderDispatchedFunction `
  --included-event-types OrderDispatched

# Subscribe to VendorStatusChanged events
az eventgrid event-subscription create `
  --name vendor-status-changed-subscription `
  --source-resource-id /subscriptions/<subscription-id>/resourceGroups/rg-fooddelivery/providers/Microsoft.EventGrid/topics/eg-fooddelivery `
  --endpoint-type azurefunction `
  --endpoint /subscriptions/<subscription-id>/resourceGroups/rg-fooddelivery/providers/Microsoft.Web/sites/func-fooddelivery-<random>/functions/VendorStatusChangedFunction `
  --included-event-types VendorStatusChanged