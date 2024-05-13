# WhoIs.Api Docker Image

This repository contains the necessary files to create a Docker image for the `dotnet 8 WhoIs.Api` API.

## Prerequisites

- Docker installed on your system. If Docker is not installed, you can download it from [Docker's official website](https://www.docker.com/get-started).

## Building the Docker Image

To build the Docker image, follow these steps:

1. Clone this repository to your local machine:

    ```bash
    git clone https://github.com/EmirhanAksoy/WhoIs.git
    ```

2. Navigate to the cloned directory:

    ```bash
    cd WhoIs.API
    ```

3. Build the Docker image using the provided Dockerfile:

    ```bash
    docker build -t whoisapi .
    ```

    This command will build the Docker image with the tag `whoisapi`.

## Running the Docker Container

Once the Docker image is built, you can run a Docker container using the following command:

```bash
docker run -d -p 32787:80 --name whois-container whoisapi
```

This command will start a container named `whois-container` from the `whoisapi` image, mapping port 8080 on the host to port 80 on the container. The API will then be accessible at `http://localhost:8080`.

## Accessing the WhoIs.Api

After the container is running, you can access the WhoIs.Api by visiting [http://localhost:32787](http://localhost:32787) in your web browser or making HTTP requests to the API endpoints.

## Additional Configuration

If you need to customize any configurations for the API, you can modify the `appsettings.json` file before building the Docker image.

## License

This project is licensed under the [MIT License](LICENSE).

---

Feel free to adjust or expand upon this README as needed for your project! Let me know if you need further assistance.
