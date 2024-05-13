# Database Seed Docker Image

This repository contains the necessary files to create a Docker image for seeding data into a database.

## Prerequisites

- Docker installed on your system. If Docker is not installed, you can download it from [Docker's official website](https://www.docker.com/get-started).
- Database server running, accessible from the Docker container.

## Usage

To use this Docker image for seeding data into your database, follow these steps:

1. Clone this repository to your local machine:

    ```bash
    git clone https://github.com/EmirhanAksoy/WhoIs.git
    ```

2. Navigate to the cloned directory:

    ```bash
    cd .\WhoIs.DBSeed\Database
    ```

3. Modify the seed data or scripts according to your database requirements.

4. Build the Docker image using the provided Dockerfile:

    ```bash
    docker build -t database_seed .
    ```

    This command will build the Docker image with the tag `database_seed`.

5. Run a Docker container from the `database_seed` image:

    ```bash
    docker run --name db-seed-container --network host database_seed
    ```

    Replace `--network host` with appropriate network settings if needed to connect to the database server.

6. The seed data will be inserted into the database as per the configured scripts.

## Configuration

If you need to customize the seed data or scripts for your database, you can modify the relevant files within the Docker image before building it.

## License

This project is licensed under the [MIT License](LICENSE).
