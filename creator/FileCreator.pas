unit FileCreator;

interface

type

  TFileCreator = class
  private const
    // макс длина строки
    maxstrlen = 1024;
    // макс число
    maxnumber = MaxInt;
    // символы для генерации строки
    symbols = 'abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.,?!:;';
  private
    fFileName: string;
    fFileSize: Int64;
    // массив для хранения строк, с помощью него создаются одинковые строки
    arst: array of string;
    function getSym: char;
    function getStr(n: integer; accu: boolean): string;
    function getNum: cardinal;
    function getNStr(n: integer): string;
    function getAnyStr: string;
    procedure addStr(aStr: string);
  public
    constructor Create(aFileName: string; aFileSize: Int64);
    destructor Destroy; override;
    procedure DoIt;
  end;

implementation

uses
  System.SysUtils;

{ TFileCreator }

// дает случайный символ из строки
function TFileCreator.getSym: char;
begin
  result := symbols[Random(Length(symbols)) + 1];
end;

// формирует случайную строку
// если accu = true, то длина строки = n
// если accu = false, то длина строки <=n
function TFileCreator.getStr(n: integer; accu: boolean): string;
var
  i, c: integer;
begin
  if accu then
    c := n
  else
    c := Random(n) + 1;
  SetLength(result, c);
  for i := 1 to c do
    result[i] := getSym;
end;

// возвращает случайное число
function TFileCreator.getNum: cardinal;
begin
  result := abs(Random(maxnumber));
  if Random > 0.5 then
    result := result * 2;
end;

// возвращает строку вида Number. String длиной n
function TFileCreator.getNStr(n: integer): string;
begin
  result := Format('%u. %s', [abs(Random(10)), getStr(n - 3, true)]);
end;

// возвращает любую строку вида Number. String
// возможно одну из существующих
function TFileCreator.getAnyStr: string;
var
  s: string;
begin
  if Random > 0.5 then
    s := arst[Random(10)];
  if s = '' then
    s := getStr(maxstrlen, false);
  // добавляю строку в массив для организации повторения строк
  addStr(s);
  result := Format('%u. %s', [getNum, s]);
end;

// добавить строку в массив в случайную позицию
procedure TFileCreator.addStr(aStr: string);
begin
  if Random > 0.5 then
    arst[Random(10)] := aStr;
end;

constructor TFileCreator.Create(aFileName: string; aFileSize: Int64);
begin
  fFileName := aFileName;
  fFileSize := aFileSize;
  SetLength(arst, 10);
end;

destructor TFileCreator.Destroy;
begin
  arst := nil;
  inherited;
end;

// запись в файл
procedure TFileCreator.DoIt;
var
  aFile: text;
  cs: Int64;
  s: string;
begin
  cs := 0;
  AssignFile(aFile, fFileName);
  FileMode := fmOpenWrite;
  Rewrite(aFile);
  // цикл до требуемого размера файла
  while cs < fFileSize do
  begin
    // если осталось меньше байт
    // по заданию достаточно кодировки ANSI
    // поэтому упрощаю - один символ = один байт
    // иначе для юникода нужно было бы SizeOf(Char)
    if fFileSize - cs < maxstrlen then
    begin
      // то генерирую строку длины до оставшегося размера
      s := getNStr(fFileSize - cs);
      cs := fFileSize;
      Write(aFile, s);
    end
    else
    begin
      // генерирую случайную строку
      s := getAnyStr;
      // если остается меньше 4 байт, а такая строка не может быть,
      // то генерирую строку меньшего размера
      // чтобы осталось на следующую
      if abs(cs + Length(s) + 2) < 4 then
        s := getNStr(maxstrlen div 2);
      cs := cs + Length(s) + 2;
      Writeln(aFile, s);
    end;
  end;
  CloseFile(aFile);
end;

end.
