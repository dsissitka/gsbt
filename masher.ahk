Gui, +AlwaysOnTop
Gui, Show, Center H270 W270, Masher

SetKeyDelay, -1

; Make the window red to signal that we're about to start.
Gui, Color, ff0000
Sleep, 10000

; Make the window green to signal that we've started.
Gui, Color, 00ff00

; Send the letter "a" to the local Mashee and the focused window 1,000 times in
; 100 ms intervals.
loop {
  ControlGet, ControlId, Hwnd, , Chrome_RenderWidgetHostHWND1, Mashee - Google Chrome
  ControlFocus, , ahk_id %ControlId%
  ControlSend, Chrome_RenderWidgetHostHWND1, a, Mashee - Google Chrome

  Send, a

  if (A_Index = 1000) {
    break
  } else {
    Sleep, 100
  }
}

; Make the window red to signal that we've finished.
Gui, Color, ff0000
Sleep, 10000

GuiClose:
  ExitApp
