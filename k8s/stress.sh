#!/bin/bash
for i in {1..10000}; do
  curl -w "\n" localhost/api/localip >> out.txt
  sleep $1
done
