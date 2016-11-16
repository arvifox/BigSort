program task1;

{$APPTYPE CONSOLE}

{$R *.res}

uses
  System.SysUtils,
  System.IOUtils,
  FileCreator in 'FileCreator.pas';

var
  filesize: Int64;
  fcreator: TFileCreator;

begin
  try
    // проверка параметров командной строки
    if (ParamCount <> 2) or
      (not TPath.HasValidPathChars(ParamStr(1), false)) or
      (not TryStrToInt64(ParamStr(2), filesize)) then
    begin
      Writeln('Incorrect parameters.');
      Exit;
    end;
    Randomize;
    // старт класса, создающего файл
    fcreator := TFileCreator.Create(ParamStr(1), filesize);
    fcreator.DoIt;
    fcreator.Free;
  except
    on E: Exception do
      Writeln(E.ClassName, ': ', E.Message);
  end;
end.
