# Use the official lightweight NGINX image
FROM nginx:alpine

# Install the 'gettext' package which provides the 'envsubst' utility.
# This allows us to substitute environment variables into our config template.
RUN apk add --no-cache gettext

# Copy our custom configuration template file from the local directory to the container.
COPY default.conf.template /etc/nginx/conf.d/default.conf.template

# The container will now start by running a script that processes this template.