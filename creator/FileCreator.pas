unit FileCreator;

interface

type

  TFileCreator = class
  private const
    // ���� ����� ������
    maxstrlen = 1024;
    // ���� �����
    maxnumber = MaxInt;
    // ������� ��� ��������� ������
    symbols = 'abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.,?!:;';
  private
    fFileName: string;
    fFileSize: Int64;
    // ������ ��� �������� �����, � ������� ���� ��������� ��������� ������
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

// ���� ��������� ������ �� ������
function TFileCreator.getSym: char;
begin
  result := symbols[Random(Length(symbols)) + 1];
end;

// ��������� ��������� ������
// ���� accu = true, �� ����� ������ = n
// ���� accu = false, �� ����� ������ <=n
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

// ���������� ��������� �����
function TFileCreator.getNum: cardinal;
begin
  result := abs(Random(maxnumber));
  if Random > 0.5 then
    result := result * 2;
end;

// ���������� ������ ���� Number. String ������ n
function TFileCreator.getNStr(n: integer): string;
begin
  result := Format('%u. %s', [abs(Random(10)), getStr(n - 3, true)]);
end;

// ���������� ����� ������ ���� Number. String
// �������� ���� �� ������������
function TFileCreator.getAnyStr: string;
var
  s: string;
begin
  if Random > 0.5 then
    s := arst[Random(10)];
  if s = '' then
    s := getStr(maxstrlen, false);
  // �������� ������ � ������ ��� ����������� ���������� �����
  addStr(s);
  result := Format('%u. %s', [getNum, s]);
end;

// �������� ������ � ������ � ��������� �������
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

// ������ � ����
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
  // ���� �� ���������� ������� �����
  while cs < fFileSize do
  begin
    // ���� �������� ������ ����
    // �� ������� ���������� ��������� ANSI
    // ������� ������� - ���� ������ = ���� ����
    // ����� ��� ������� ����� ���� �� SizeOf(Char)
    if fFileSize - cs < maxstrlen then
    begin
      // �� ��������� ������ ����� �� ����������� �������
      s := getNStr(fFileSize - cs);
      cs := fFileSize;
      Write(aFile, s);
    end
    else
    begin
      // ��������� ��������� ������
      s := getAnyStr;
      // ���� �������� ������ 4 ����, � ����� ������ �� ����� ����,
      // �� ��������� ������ �������� �������
      // ����� �������� �� ���������
      if abs(cs + Length(s) + 2) < 4 then
        s := getNStr(maxstrlen div 2);
      cs := cs + Length(s) + 2;
      Writeln(aFile, s);
    end;
  end;
  CloseFile(aFile);
end;

end.
