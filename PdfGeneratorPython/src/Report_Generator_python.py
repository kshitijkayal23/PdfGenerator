import io
from PIL import Image
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.image as mpimg
from matplotlib.backends.backend_pdf import PdfPages
import cv2

def generate_pdf_report(csv_file_path, logo_img_bytes, cover_img_bytes, site_info):
    # Load the CSV file
    data = pd.read_csv(csv_file_path)

    # Decode the logo image from byte
    logo_img = Image.open(io.BytesIO(logo_img_bytes))
    cover_img = Image.open(io.BytesIO(cover_img_bytes))

    # Create an in-memory PDF
    pdf_output = io.BytesIO()

    # Extract necessary columns for analysis
    manufacturer_counts = data['PACKAGE MANUFACTURER (ID)'].value_counts()
    protocol_counts = data['PACKAGE PROTOCOL'].value_counts()
    hart_manufacturer_counts = data[data['PACKAGE PROTOCOL'] == 'HART 5']['PACKAGE MANUFACTURER (ID)'].value_counts()
    hart_device_type_counts = data[data['PACKAGE PROTOCOL'] == 'HART 5']['DEVICE TYPE'].value_counts()

    # Create a PDF with A4 page size
    with PdfPages(pdf_output) as pdf:
        # Add Cover Page
        fig, ax = plt.subplots(figsize=(8.27, 11.69))  # A4 size in inches
        ax.axis('off')

        # logo_img = mpimg.imread(logo_img_bytes)
        ax.imshow(logo_img, extent=[0.1, 0.9, 0.75, 1.0], aspect='equal')
        
        # Add the main cover image in the center with maintained aspect ratio
        # cover_img = mpimg.imread(cover_img_bytes)
        ax.imshow(cover_img, extent=[0.2, 0.8, 0.35, 0.65], aspect='equal')

        # Display site information at the bottom
        ax.text(0.5, 0.2, site_info, ha='center', va='center', fontsize=12)
        ax.set_title("Install Base Report", fontsize=24, weight='bold')

        pdf.savefig()
        plt.close()

        # Pie Chart: Total Number of Instruments by Manufacturer
        plt.figure(figsize=(8.27, 11.69))
        plt.title("Total Instruments by Manufacturer", fontsize=16, weight='bold')
        manufacturer_counts.plot(kind='pie', labels=manufacturer_counts.index, autopct=lambda p: f'{int(p * sum(manufacturer_counts) / 100)}', startangle=140)
        plt.tight_layout()
        pdf.savefig()
        plt.close()

        # Pie Chart: Total Number of Instruments by Protocol
        plt.figure(figsize=(8.27, 11.69))
        plt.title("Total Instruments by Protocol", fontsize=16, weight='bold')
        protocol_counts.plot(kind='pie', labels=protocol_counts.index, autopct=lambda p: f'{int(p * sum(protocol_counts) / 100)}', startangle=140)
        plt.tight_layout()
        pdf.savefig()
        plt.close()
        
        # Pie Chart: HART Instruments by Manufacturer
        plt.figure(figsize=(8.27, 11.69))
        plt.title("HART Instruments by Manufacturer", fontsize=16, weight='bold')
        hart_manufacturer_counts.plot(kind='pie', labels=hart_manufacturer_counts.index, autopct=lambda p: f'{int(p * sum(hart_manufacturer_counts) / 100)}', startangle=140)
        plt.tight_layout()
        pdf.savefig()
        plt.close()
        
        # Bar Graph: HART Instruments by Device Type
        plt.figure(figsize=(8.27, 11.69))
        plt.title("HART Instruments by Device Type", fontsize=16, weight='bold')
        hart_device_type_counts.plot(kind='bar', color='skyblue')
        plt.xlabel("Device Type", fontsize=12)
        plt.ylabel("Count", fontsize=12)
        plt.xticks(rotation=45, ha='right')
        plt.tight_layout()
        pdf.savefig()
        plt.close()

        # Create a pivot table to count similar devices
        pivot_table = data.pivot_table(index=['DEVICE TYPE', 'PACKAGE MANUFACTURER (ID)'], 
                                       columns='PACKAGE PROTOCOL', 
                                       aggfunc='size', 
                                       fill_value=0).reset_index()
        pivot_table.columns.name = None  # Remove pivot table index name

        # Split the table if it doesn't fit on a single page
        rows_per_page = 45 # Number of rows to fit per page
        num_rows = pivot_table.shape[0]
        font_size = 8  # Initial font size for table

        for start_row in range(0, num_rows, rows_per_page):
            end_row = min(start_row + rows_per_page, num_rows)
            table_page_data = pivot_table.iloc[start_row:end_row]

            fig, ax = plt.subplots(figsize=(8.27, 11.69))  # A4 size
            ax.axis('off')
            plt.title("Device Information (Count of Similar Devices)", fontsize=16, weight='bold')

            table = ax.table(cellText=table_page_data.values, 
                             colLabels=table_page_data.columns, 
                             cellLoc='center', 
                             loc='center')
            
            # Adjust font size to fit table within the box
            table.auto_set_font_size(False)
            table.set_fontsize(font_size)
            table.auto_set_column_width(col=list(range(len(table_page_data.columns))))
            
            # Dynamically reduce font size if text is not fitting
            while table.get_celld()[(1, 1)].get_text().get_fontsize() > 6 and any(cell.get_text().get_fontsize() < font_size for cell in table.get_celld().values()):
                font_size -= 1
                table.set_fontsize(font_size)

            pdf.savefig()  # Save each table chunk to a new PDF page
            plt.close()

    # Get the binary data of the PDF and return it
    return pdf_output.getvalue()
