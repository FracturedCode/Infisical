FROM infisical/infisical:v0.83.0-postgres
RUN cp /backend/docker-entrypoint.sh /tmp/docker-entrypoint.sh
RUN echo "sleep 20" > /backend/docker-entrypoint.sh
RUN cat /tmp/docker-entrypoint.sh >> /backend/docker-entrypoint.sh