# printf "USER:$(openssl passwd -crypt PASSWORD)\n" >> .htpasswd
docker stop -t 0 ai
docker rm ai
docker run --name ai -d \
       -p 80:80 \
       -p 81:81 \
       -v $PWD:/opt/www/static \
       -v $PWD/app:/data/app \
       -v $PWD:/opt/notebook \
       -v $PWD/conf/nginx.conf:/usr/local/openresty/nginx/conf/nginx.conf \
       -v $PWD/conf/redis.conf:/etc/redis/redis.conf \
       -v $PWD/conf/.htpasswd:/etc/nginx/.htpasswd \
       --sysctl net.core.somaxconn=65536 \
       caapi/ai
echo "jupyter listen on 80; webdav listion on 81"