mergeInto(LibraryManager.library, {
    GeneratePDFFromUnity: function(base64Image, name) {
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF('landscape', 'mm', 'a4');
        
        // Convertir la imagen base64 a un formato que jsPDF pueda utilizar
        var imgData = UTF8ToString(base64Image);

        pdf.addImage(imgData, 'PNG', 0, 0, 297, 210); // Acomoda el tamaño según sea necesario

        // Descargar el PDF
        pdf.save(UTF8ToString(name)+"(GameOfDuck.com).pdf");
    }
});