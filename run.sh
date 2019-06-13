# printf "USER:$(openssl passwd -crypt PASSWORD)\n" >> .htpasswd
docker stop -t 0 ai
docker rm ai
docker run --name ai -d \
       -p 80:80 \
       -p 81:81 \
       -p 8000:9999 \
       -p 5001:5000 \
       -p 6380:6379 \
       -v $PWD:/opt/www/static \
       -v $PWD/app:/data/app \
       -v $PWD:/opt/notebook \
       -v $PWD/conf/nginx.conf:/usr/local/openresty/nginx/conf/nginx.conf \
       -v $PWD/conf/redis.conf:/etc/redis/redis.conf \
       -v $PWD/conf/.htpasswd:/etc/nginx/.htpasswd \
       --sysctl net.core.somaxconn=65536 \
       caapi/ai
echo "dav start, listion on 81; jupyter start, listen on 80"