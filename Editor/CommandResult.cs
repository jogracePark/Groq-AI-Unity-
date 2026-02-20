using System;
using System.Collections.Generic;

/// <summary>
/// Unity 명령 실행 결과를 나타내는 데이터 구조입니다.
/// </summary>
[Serializable]
public class CommandResult
{
    /// <summary>
    /// 명령 실행 성공 여부입니다.
    /// </summary>
    public bool success;
    /// <summary>
    /// 명령 실행에 대한 메시지입니다 (성공 또는 실패).
    /// </summary>
    public string message;
    /// <summary>
    /// 실행된 명령의 타입입니다.
    /// </summary>
    public string commandType;
    /// <summary>
    /// 명령 실행 시 전달된 원본 매개변수 (JSON 문자열)입니다.
    /// </summary>
    public string parameters; // Original parameters for context
    /// <summary>
    /// 명령 실행으로 인한 특정 출력 (예: 검색 결과)입니다.
    /// </summary>
    public string output; // Any specific output from the command
    /// <summary>
    /// 추가적인 구조화된 데이터입니다.
    /// </summary>
    public Dictionary<string, string> data; // Additional structured data
}
