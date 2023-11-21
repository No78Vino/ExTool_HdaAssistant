param(
    $ExeAbsolutePath
)

Start-Process -FilePath $ExeAbsolutePath --enable-remote-scripting

print('Substance Painter Launch Success!');