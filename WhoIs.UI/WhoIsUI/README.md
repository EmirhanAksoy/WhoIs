# WhoIs.UI Docker Image

This repository contains the necessary files to create a Docker image for the WhoIs.UI Angular 14 project.

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
    cd .\WhoIs.UI\WhoIsUI
    ```

3. Build the Docker image using the provided Dockerfile:

    ```bash
    docker build -t whoisui .
    ```

    This command will build the Docker image with the tag `whoisui`.

## Running the Docker Container

Once the Docker image is built, you can run a Docker container using the following command:

```bash
docker run -d -p 4000:80 --name whoisui-container whoisui
```

This command will start a container named `whoisui-container` from the `whoisui` image, mapping port 4000 on the host to port 80 on the container. The Angular project will then be accessible at `http://localhost:4000`.

## Accessing the WhoIs.UI

After the container is running, you can access the WhoIs.UI by visiting [http://localhost:4000](http://localhost:4000) in your web browser.

## Additional Configuration

If you need to customize any configurations for the Angular project, you can modify the relevant configuration files before building the Docker image.

## License

This project is licensed under the [MIT License](LICENSE).
