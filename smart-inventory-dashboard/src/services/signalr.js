import * as signalR from '@microsoft/signalr'

export function createInventoryConnection(onStockUpdated) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:8080/hubs/inventory', {
      withCredentials: false,
    })
    .withAutomaticReconnect()
    .build()

  connection.on('StockUpdated', onStockUpdated)

  return connection
}