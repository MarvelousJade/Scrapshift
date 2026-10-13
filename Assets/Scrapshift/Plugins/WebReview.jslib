mergeInto(LibraryManager.library, {
  ScrapshiftReport: function (json) { if (window.scrapshiftReport) window.scrapshiftReport(UTF8ToString(json)); }
});
