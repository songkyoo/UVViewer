# UV Viewer for Unity Editor
UVViewerWindow는 유니티 에디터에서 메시의 UV를 확인할 수 있는 에디터 윈도우 확장 클래스입니다. 유니티 5.4 버전 이상이 필요합니다.

![](Screenshot.png)

## 설치
[릴리스 페이지](https://github.com/songkyoo/UVViewer/releases)에서 유니티 패키지를 다운로드 받아 임포트하거나 [Assets/Plugins](Assets/Plugins) 폴더를 프로젝트로 복사하면 메뉴바의 Window 항목에 UV Viewer가 추가됩니다. 해당 항목을 실행하면 에디터 윈도우가 생성됩니다.

## 사용법
### 메시 설정
Mesh 항목이 Selected Object일 경우 메시 혹은 MeshFilter, SkinnedMeshRenderer 컴포넌트를 포함하는 게임 오브젝트를 선택하면 UV를 표시합니다.

Mesh 항목을 Custom으로 설정하면 메시를 직접 설정할 수 있습니다. 오브젝트를 뷰 영역으로 드래그 앤 드롭해도 동일한 동작을 수행합니다. 드래그 가능한 오브젝트는 메시 혹은 MeshFilter, SkinnedMeshRenderer 컴포넌트를 포함하는 게임 오브젝트입니다.

### 텍스처 설정
Mesh 항목이 Selected Object일 경우 선택한 게임 오브젝트가 MeshRenderer, SkinnedMeshRenderer 컴포넌트를 포함한다면 Texture 항목을 Materials로 설정했을 경우 렌더러에 포함된 머티리얼의 텍스처를 선택할 수 있습니다.

Texture 항목을 Custom으로 설정하면 텍스처를 직접 설정할 수 있습니다. 텍스처를 뷰 영역으로 드래그 앤 드롭해도 동일한 동작을 수행합니다.

### 에디터 스크립트에서 접근하기
에디터 스크립트에서 생성한 메시 혹은 텍스처를 설정할 수 있습니다. 다음 코드는 윈도우가 표시되고 있다면 표시되고 있는 윈도우에 값을 설정하고 표시되는 윈도우가 없다면 새로운 윈도우를 생성하고 값을 설정합니다.

```csharp
using Macaron.UVViewer.Editor;

class EditorClass
{
    void SetCustom(Mesh mesh, Texture texture)
    {
        // 메시 설정.
        UVViewerWindow.ShowWindow().SetCustomMesh(mesh);

        // 텍스처 설정.
        UVViewerWindow.ShowWindow().SetCustomTexture(texture);
    }
}
```

`SetCustomMesh`, `SetCustomTexture` 메서드를 호출하면 Mesh, Texture 항목이 Custom으로 변경되고 호출한 값으로 Source가 설정됩니다. 각 메서드는 개별적으로 호출될 수 있습니다.

## 제한 사항
1. 표시되는 UV와 텍스처는 원본에 대한 변경을 자동으로 반영하지 않습니다. 갱신이 필요할 경우 Mesh, Texture 항목의 Reload 버튼을 눌러 다시 로드해야합니다.

2. Geometry 셰이더가 지원되지 않는 경우 Line Thickness 항목을 사용할 수 없고, HiDPI 환경에서 뷰 영역이 저해상도로 표시됩니다.
