# E-Commerce Application

## Description
This is a microservices-based e-commerce application designed to simulate online shopping functionalities. It allows users to browse products, add them to a cart, and place orders.

## Project Structure
The application is composed of the following microservices:
- **CartService**: Manages shopping cart operations, such as adding, removing, and viewing items in the cart.
- **OrderService**: Handles order processing, including creating orders, managing order status, and retrieving order history.
- **ProductService**: Manages product information, such as product listings, details, and inventory.

## Getting Started
To build and run the application, ensure you have Docker and Docker Compose installed on your system. Follow these steps:

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd <repository-directory>
   ```

2. **Build and run the services using Docker Compose:**
   ```bash
   docker-compose up --build
   ```
   This command will build the Docker images for each service and start the containers.

3. **Access the application:**
   Once the services are running, you can access the application through the exposed API endpoints.

## API Endpoints
This section provides a summary of the available API endpoints for each service. (Note: This section can be expanded with more detailed endpoint documentation.)

### CartService
- `GET /cart/{userId}`: Retrieves the cart for a specific user.
- `POST /cart/{userId}/items`: Adds an item to the user's cart.
- `DELETE /cart/{userId}/items/{itemId}`: Removes an item from the user's cart.

### OrderService
- `POST /orders`: Creates a new order.
- `GET /orders/{orderId}`: Retrieves details for a specific order.
- `GET /users/{userId}/orders`: Retrieves all orders for a specific user.

### ProductService
- `GET /products`: Retrieves a list of all products.
- `GET /products/{productId}`: Retrieves details for a specific product.
