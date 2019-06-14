docker stop -t 0 ai
docker rm ai
docker run --name ai -d  -p 80:80 -p 81:81  -v C:\2019-assignments:/opt/www/static -v C:\2019-assignments:/opt/notebook -v C:\2019-assignments\app:/data/app -v C:\2019-assignments\conf\nginx.conf:/usr/local/openresty/nginx/conf/nginx.conf -v C:\2019-assignments\conf\redis.conf:/usr/local/openresty/nginx/conf/redis.conf -v C:\2019-assignments\conf\.htpasswd:/usr/local/openresty/nginx/conf/.htpasswd  --sysctl net.core.somaxconn=65536 caapi/ai
$url = 'http://localhost'
try {
    Start-Process "chrome.exe" $url
}
catch {
    Start-Process $url
}