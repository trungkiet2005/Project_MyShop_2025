using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Printing;
using Project_MyShop_2025.Core.Models;
using Project_MyShop_2025.Core.Services.Interfaces;
using Project_MyShop_2025.Views.PrintTemplates;
using System;
using System.Threading.Tasks;
using Windows.Graphics.Printing;

namespace Project_MyShop_2025.Services
{
    public class PrintService : IPrintService
    {
        private PrintManager? _printManager;
        private PrintDocument? _printDocument;
        private IPrintDocumentSource? _printDocumentSource;
        private OrderInvoicePage? _printablePage;
        private Window? _window;
        private nint _hWnd;

        public bool IsPrintingSupported => PrintManager.IsSupported();

        public PrintService()
        {
            // We need the main window to initialize printing
            // Accessing App.Window directly if available or pass it in.
            // In this specific app structure, let's assume we can get it or we initialize lazily.
        }

        public async Task PrintOrderAsync(Order order)
        {
            // Get the window handle
            if (App.Current is App app && app.Window != null)
            {
                _window = app.Window;
                _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
                
                // Register for PrintTaskRequested
                _printManager = PrintManagerInterop.GetForWindow(_hWnd);
                _printManager.PrintTaskRequested += PrintTaskRequested;

                // Create the content
                _printablePage = new OrderInvoicePage(order);
                
                // Show Print UI
                await PrintManagerInterop.ShowPrintUIForWindowAsync(_hWnd);
            }
        }

        private void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            var printTask = args.Request.CreatePrintTask("Order Invoice", sourceRequested =>
            {
                _printDocument = new PrintDocument();
                _printDocumentSource = _printDocument.DocumentSource;
                
                _printDocument.Paginate += PrintDocument_Paginate;
                _printDocument.GetPreviewPage += PrintDocument_GetPreviewPage;
                _printDocument.AddPages += PrintDocument_AddPages;

                sourceRequested.SetSource(_printDocumentSource);
            });
            
            printTask.Completed += PrintTask_Completed;
        }

        private void PrintDocument_Paginate(object sender, PaginateEventArgs e)
        {
            // Setup content for printing
            _printDocument.SetPreviewPageCount(1, PreviewPageCountType.Final);
        }

        private void PrintDocument_GetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            _printDocument.SetPreviewPage(e.PageNumber, _printablePage);
        }

        private void PrintDocument_AddPages(object sender, AddPagesEventArgs e)
        {
            _printDocument.AddPage(_printablePage);
            _printDocument.AddPagesComplete();
        }

        private void PrintTask_Completed(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
            // Cleanup
            if (_printManager != null)
            {
                _printManager.PrintTaskRequested -= PrintTaskRequested;
                _printManager = null;
            }
            
            _printDocument = null;
            _printDocumentSource = null;
            _printablePage = null;
        }
    }
}
