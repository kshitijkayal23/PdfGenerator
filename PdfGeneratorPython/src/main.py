from flask import Flask, jsonify, send_file, request
from werkzeug.exceptions import BadRequest
import io
import os
import pandas as pd
from flask_wtf import CSRFProtect
from Report_Generator_python import generate_pdf_report

app = Flask(__name__)
csrf = CSRFProtect()

log_level = os.getenv("LOG_LEVEL", "INFO")
app.logger.setLevel(log_level)

@app.route('/api/generate', methods=['POST'])
def pdfGeneration():
    app.logger.debug("pdfGeneration called")
    try:
        # Check if the files are present in the request
        if 'csvFile' not in request.files or 'coverImage' not in request.files or 'logoImage' not in request.files:
            return jsonify({"error": "No file part"}), 400

        # Get the files from the request
        csvFile = request.files['csvFile']
        coverImage = request.files['coverImage']
        logoImage = request.files['logoImage']
        siteinfo = request.form.get('siteinfo')
        # cover_image_path='./cover_image.jpg'
        # logo_path='./logo.png'


        # Ensure the files are not empty
        if csvFile.filename == '' or coverImage.filename == '' or logoImage.filename == '':
            return jsonify({"error": "No selected file"}), 400
        
        # site_info = "testing"  # You should fetch this value from the actual request if passed

        # Ensure site_info is provided
        if not siteinfo:
            raise BadRequest("Missing required input data: 'site_info'")

        # Read the image files as byte streams
        logo_img_bytes = logoImage.read()
        cover_img_bytes = coverImage.read()

        # Debugging: Check if we received the images properly
        app.logger.debug(f"Logo Image Bytes Length: {len(logo_img_bytes)}")
        app.logger.debug(f"Cover Image Bytes Length: {len(cover_img_bytes)}")

        # Call the PDF generation function with byte data
        pdf_content = generate_pdf_report(
            csvFile,
            logo_img_bytes,
            cover_img_bytes,
            siteinfo
        )

        # Return the generated PDF as a response
        return send_file(io.BytesIO(pdf_content), mimetype='application/pdf', as_attachment=True, download_name='report.pdf')

    except BadRequest as e:
        app.logger.error("Bad request: %s", e)
        return jsonify({'error': str(e)}), 400

    except Exception as e:
        app.logger.error("Something went wrong: %s", e)
        return jsonify({'error': 'Error occurred: ' + str(e)}), 500


if __name__ == '__main__':
    app.logger.info('Starting Flask app')
    app.run(debug=False)
    csrf.init_app(app)
