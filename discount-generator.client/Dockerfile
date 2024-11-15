FROM node:18-alpine

WORKDIR /app

COPY package*.json ./

RUN npm install

COPY . .

ARG REACT_APP_SIGNALR_SERVER=http://localhost:7083/discount-hub
ENV REACT_APP_SIGNALR_SERVER=$REACT_APP_SIGNALR_SERVER

RUN npm run build

FROM nginx:alpine

COPY --from=0 /app/build /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]