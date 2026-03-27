FROM node:22-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./

# Cài đặt sạch và ép nhận React 19
RUN npm install --legacy-peer-deps

# Copy code
COPY . .

# Build dự án (xóa rác cũ)
RUN rm -rf dist && npm run build

FROM nginx:stable-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

# SỬA DÒNG NÀY (Thêm nginx -g vào)
CMD ["nginx", "-g", "daemon off;"]