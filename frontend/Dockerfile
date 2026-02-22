# Build Stage
FROM node:18-slim as build
WORKDIR /app
COPY package*.json ./
RUN npm config set registry https://registry.npmjs.org/
RUN npm install --legacy-peer-deps
COPY . .
RUN npm run build

# Production Stage
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
