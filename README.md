EXIF Copy and Move
==================

This project contains WPF application and command line tool
to move or copy photo image files based on its date.

Date can be retrieved from EXIF or file name.

Destination directory
---------------------

The destination directory can contain the `{date:date-format}` tag.

The `date-format` part can be any of
[Standard Date and Time Format Strings](https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx) or
[Custom Date and Time Format Strings](https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx).

For instance, the destination directory
`D:\images\{date:yyyy/yyyy-MM/yyyy-MM-dd}`
will move or copy image files to the directory such as
`D:\images\2016\2016-05\2016-05-30`.
