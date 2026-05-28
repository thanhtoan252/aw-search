#!/bin/sh
set -eu

export API_URL="${API_URL:-}"

envsubst '${API_URL}' \
  < /usr/share/nginx/html/runtime-config.template.js \
  > /usr/share/nginx/html/runtime-config.js
