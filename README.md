# WhoIs Application

Welcome to the WhoIs application, a face recognition and identification system!

## Overview

WhoIs is an application designed for identifying individuals from a collection of images. Users can upload a list of images in a zip file, and after some time, the images will be processed. The processed images can then be viewed in the "Faces" tab of the UI. Users can also set face names to search for images containing specific faces.

## Features

- Upload a list of images with a zip file.
- Process images to detect faces.
- View processed images and identified faces in the "Faces" tab of the UI.
- Set face names to search for images containing specific faces.

## Technology Stack

The WhoIs application utilizes the following technologies:

- **Backend**: 
  - .NET 8 (Web API)
  - MSSQL (Database)
  - Python (Face Recognition Service)
  - Docker
  - Seq

- **Frontend**:
  - Angular 14

## Repositories

To create Docker images for the various components of the WhoIs application, please refer to the following READMEs:

- **Web API Project**: 
  - [README for WhoIs API Project](https://github.com/EmirhanAksoy/WhoIs/blob/main/WhoIs.API/README.md)

- **UI Project**: 
  - [README for WhoIs UI Project](https://github.com/EmirhanAksoy/WhoIs/blob/main/WhoIs.UI/WhoIsUI/README.md)

- **Database Seed Project**: 
  - [README for Database Seed Project](https://github.com/EmirhanAksoy/WhoIs/blob/main/WhoIs.DBSeed/Database/README.md)

- **Face Recognition Service**:
  - [README for Face Recognition Service](https://github.com/EmirhanAksoy/face_recognition_distinct_faces_db/blob/master/README.md)

## Running the Application

After creating all Docker images for the components, you can run the entire application using Docker Compose. Simply run the following command:

```bash
docker-compose up
```

## Uploading Images as a Zip File

To upload a list of images as a zip file, you can use CURL. Here's an example CURL command:

```bash
curl -X 'POST' \
  'http://localhost:32787/image-bulk-upload' \
  -H 'accept: */*' \
  -H 'Content-Type: multipart/form-data' \
  -F 'zipFile=@Family.zip;type=application/x-zip-compressed'
```

![Alt Text](https://media.giphy.com/media/v1.Y2lkPTc5MGI3NjExMXc5c3AwamZyMG53ZXN3eXk2emJ0c3pqZXY1bWJlMmQ2Z215M2k0ayZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/p5jCcrjNBUDK2cJDKG/source.gif)


This command will start all the containers required for the WhoIs application, including the backend, frontend, database, and face recognition service.

## Contributing

Contributions are welcome! If you'd like to contribute to the WhoIs application, feel free to submit pull requests or open issues in the respective repositories.

## License

This project is licensed under the [MIT License](LICENSE).
