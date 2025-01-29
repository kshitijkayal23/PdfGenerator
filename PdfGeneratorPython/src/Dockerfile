# Use the latest Python runtime base image (currently 3.11)
FROM python:3.10-slim-buster

# Set the working directory in the container
WORKDIR /app

# Copy the current directory contents into the container at /app
COPY . .

# Install the required dependencies from the requirements.txt file
RUN pip install --no-cache-dir -r requirements.txt

# Install latest setuptools for security updates (if needed)
# RUN pip install --no-cache-dir --upgrade setuptools

# Expose the port that Flask is running on
EXPOSE 5000

# Define environment variables for Flask to run in production
ENV FLASK_ENV=production
ENV FLASK_APP=main.py
ENV LOG_LEVEL=DEBUG

# Run the Flask app when the container launches
CMD ["flask", "run", "--host=0.0.0.0"]