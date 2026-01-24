let quill;

window.initQuill = (editorId) => {
    quill = new Quill(`#${editorId}`, {
        theme: "snow",
        modules: {
            toolbar: [
                ["bold", "italic", "underline"],
                [{ list: "ordered" }, { list: "bullet" }],
                [{ header: [1, 2, false] }],
                ["clean"]
            ]
        }
    });
};

window.getQuillHtml = () => {
    if (!quill) {
        return "";
    }
    return quill.root.innerHTML;
};

window.setQuillHtml = (html) => {
    if (!quill) {
        return;
    }
    quill.root.innerHTML = html;
};

window.setupQuillTextChange = (dotNetRef) => {
    if (!quill) {
        return;
    }
    quill.on('text-change', function() {
        const text = quill.getText();
        dotNetRef.invokeMethodAsync('UpdateWordCount', text);
    });
};

window.generatePdfFromHtml = (htmlContent, fileName) => {
    try {
        // Create a temporary div to render HTML
        const tempDiv = document.createElement('div');
        tempDiv.style.position = 'absolute';
        tempDiv.style.left = '-9999px';
        tempDiv.style.width = '210mm'; // A4 width
        tempDiv.innerHTML = htmlContent;
        document.body.appendChild(tempDiv);
        
        // Use html2canvas to convert HTML to canvas, then jsPDF to create PDF
        html2canvas(tempDiv, {
            scale: 2,
            useCORS: true,
            logging: false
        }).then(canvas => {
            const imgData = canvas.toDataURL('image/png');
            const { jsPDF } = window.jspdf;
            const pdf = new jsPDF('p', 'mm', 'a4');
            
            const imgWidth = 210; // A4 width in mm
            const pageHeight = 297; // A4 height in mm
            const imgHeight = (canvas.height * imgWidth) / canvas.width;
            let heightLeft = imgHeight;
            let position = 0;
            
            pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
            heightLeft -= pageHeight;
            
            while (heightLeft >= 0) {
                position = heightLeft - imgHeight;
                pdf.addPage();
                pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
                heightLeft -= pageHeight;
            }
            
            // Save the PDF
            pdf.save(fileName);
            
            // Clean up
            document.body.removeChild(tempDiv);
        }).catch(error => {
            console.error('Error generating PDF:', error);
            alert('Error generating PDF: ' + error.message);
            document.body.removeChild(tempDiv);
        });
    } catch (error) {
        console.error('Error in PDF generation:', error);
        alert('Error generating PDF. Please ensure jsPDF and html2canvas libraries are loaded.');
    }
};