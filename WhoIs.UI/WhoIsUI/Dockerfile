# Stage 1: Build Angular application
FROM node:14.17.0-alpine as build

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm install

COPY . .
RUN npm run build --prod

# Stage 2: Serve Angular application with Nginx
FROM nginx:1.21.0-alpine

COPY --from=build /app/dist/* /usr/share/nginx/html/
