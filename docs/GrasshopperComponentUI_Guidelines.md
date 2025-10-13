# Grasshopper �R���|�[�l���g UI �f�U�C���K�C�h���C��

�ŏI�X�V: 2025-01-13

## �T�v

���̃h�L�������g�́AFEMur �v���W�F�N�g�ɂ����� Grasshopper �R���|�[�l���g�̃J�X�^��UI����������ۂ̃f�U�C���K�C�h���C���ƃR�[�f�B���O�K����߂���̂ł��B

## �ڎ�

1. [UI���C�A�E�g�萔](#ui���C�A�E�g�萔)
2. [�R���|�[�l���g�\��](#�R���|�[�l���g�\��)
3. [UI�v�f�̎���](#ui�v�f�̎���)
4. [�R�[�f�B���O�K��](#�R�[�f�B���O�K��)
5. [������](#������)
6. [�`�F�b�N���X�g](#�`�F�b�N���X�g)

---

## UI���C�A�E�g�萔

### ��{����

- ���ׂẴ��C�A�E�g�֘A�̐��l�͒萔�Ƃ��Ē�`����
- �萔���͖��������m�ɕ�����悤�ɖ�������
- �J�e�S�����ƂɃO���[�v�����ĊǗ�����
- �P�ʂ͊�{�I�Ƀs�N�Z���ifloat�^�j

### �W���I�Ȓ萔�Z�b�g

�ȉ��́A�W�J�\�ȃ^�u���j���[�����R���|�[�l���g�̕W���I�Ȓ萔�Z�b�g�ł��F

**�R���|�[�l���g���E����̋���**
- `COMPONENT_MARGIN_HORIZONTAL = 2f` : ���E�̗]��
- `COMPONENT_MARGIN_VERTICAL = 4f` : �㉺�̗]��

**�^�u�ݒ�**
- `TAB_HEIGHT = 14f` : �^�u�̍���
- `TAB_MARGIN_TOP = 4f` : �^�u�㕔�̗]��

**���j���[���̗]���ƃT�C�Y**
- `MENU_LEFT_MARGIN = 5f` : ���j���[�����̗]��
- `MENU_TOP_PADDING = 5f` : �^�u�����烁�j���[�J�n�܂ł̗]��

**�`�F�b�N�{�b�N�X�E���W�I�{�^���ݒ�**
- `CONTROL_SIZE = 10f` : �`�F�b�N�{�b�N�X�E���W�I�{�^���̃T�C�Y
- `CONTROL_RIGHT_MARGIN = 25f` : �E�[����̋���
- `CONTROL_FILL_MARGIN = 2f` : �h��Ԃ����̓����]���i�`�F�b�N�{�b�N�X�j
- `RADIO_FILL_MARGIN = 3f` : �h��Ԃ����̓����]���i���W�I�{�^���j

**�s�Ԋu**
- `LINE_HEIGHT_NORMAL = 14f` : �ʏ�̍s�Ԋu
- `LINE_HEIGHT_SECTION = 18f` : �Z�N�V�����Ԃ̍s�Ԋu

**���j���[�S�̂̍���**
- `MENU_CONTENT_HEIGHT` : �W�J���̃��j���[�R���e���c�̍����i���e�ɉ����Čv�Z�j

**�F�ݒ�**
- `TAB_BACKGROUND_COLOR = Color.FromArgb(80, 80, 80)` : �^�u�w�i�F�i�Z���O���[�j
- `CONTROL_FILL_COLOR = Color.FromArgb(80, 80, 80)` : �`�F�b�N�E�I�����̓h��Ԃ��F
- `TEXT_COLOR = Color.Black` : �e�L�X�g�F
- `TAB_TEXT_COLOR = Color.White` : �^�u�e�L�X�g�F

### �萔�̖����K��

| �J�e�S�� | �v���t�B�b�N�X/�T�t�B�b�N�X | �� |
|---------|---------------------------|-----|
| �}�[�W�� | `MARGIN_` | `COMPONENT_MARGIN_HORIZONTAL` |
| �p�f�B���O | `PADDING_` | `MENU_TOP_PADDING` |
| ���� | `HEIGHT_` | `TAB_HEIGHT`, `LINE_HEIGHT_NORMAL` |
| �T�C�Y | `SIZE_` | `CONTROL_SIZE` |
| �F | `_COLOR` (�T�t�B�b�N�X) | `TAB_BACKGROUND_COLOR` |

---

## �R���|�[�l���g�\��

### ��{�N���X�\��

�R���|�[�l���g��2�̃N���X����\������܂��F

1. **�R���|�[�l���g�N���X** (`GH_Component` ���p��)
   - ���W�b�N�ƃf�[�^���Ǘ�
   - ���́E�o�̓p�����[�^�̒�`
   - �v�Z�����̎���

2. **�����N���X** (`GH_ComponentAttributes` ���p��)
   - UI�̕`��ƃ��C�A�E�g
   - ���[�U�[�C���^���N�V�����̏���

### �R���|�[�l���g�N���X�̃e���v���[�g

�R���|�[�l���g�N���X��: `{�@�\��}` (��: `SectionForceView`)

�����N���X��: `{�R���|�[�l���g��}Attributes` (��: `SectionForceViewAttributes`)

### �����N���X�̊�{�\��

�����N���X�͈ȉ��̃��[�W�����ō\�����܂��F

1. `UI Layout Constants` - UI�萔
2. `Fields` - �t�B�[���h
3. `Constructor` - �R���X�g���N�^
4. `Layout Methods` - ���C�A�E�g���\�b�h
5. `Rendering Methods` - �`�惁�\�b�h
6. `Event Handlers` - �C�x���g�n���h��

---

## UI�v�f�̎���

### 1. �W�J�\�ȃ^�u

#### ���C�A�E�g�v�Z

�^�u�̈ʒu�ƃT�C�Y�� `Layout()` ���\�b�h�Ōv�Z���܂��F

**�|�C���g:**
- �W�J��Ԃɉ����� `extraHeight` ���v�Z
- �^�u�̈ʒu�� `bounds.Bottom - extraHeight + TAB_MARGIN_TOP`
- �W�J���� `Layout()` ���ČĂяo��

#### �^�u�̕`��

�^�u�͈ȉ��̏����ŕ`�悵�܂��F

1. `GH_Capsule` �Ŋ�{�`���`��
2. �J�X�^���w�i�F�ŏ㏑��
3. �e�L�X�g�𒆉������ŕ`��

**���ӓ_:**
- `GH_Capsule` �͕K�� `Dispose()` ����
- �G���[/�x������ `GH_Palette` ��ύX

### 2. �`�F�b�N�{�b�N�X

#### �f�U�C���d�l

- �T�C�Y: `CONTROL_SIZE �~ CONTROL_SIZE` �̐����`
- ���`�F�b�N: ���w�i�ɍ��g
- �`�F�b�N�ς�: ������ `CONTROL_FILL_COLOR` �œh��Ԃ�
- �����̗]��: `CONTROL_FILL_MARGIN`

#### �z�u

- �E�[���� `CONTROL_RIGHT_MARGIN` �̈ʒu
- ���x���͍��[���� `MENU_LEFT_MARGIN` �̈ʒu

#### �N���b�N����

�g�O������iON/OFF�؂�ւ��j���������܂��B

### 3. ���W�I�{�^��

#### �f�U�C���d�l

- �T�C�Y: `CONTROL_SIZE �~ CONTROL_SIZE` �̉~�`
- ���I��: ���w�i�ɍ��g�̉~
- �I���ς�: ������ `CONTROL_FILL_COLOR` �œh��Ԃ����������~
- �����̗]��: `RADIO_FILL_MARGIN`
- �A���`�G�C���A�X: �L���ɂ��Ċ��炩�ɕ`��

#### �I�����̊Ǘ�

�񋓌^�ienum�j�őI�������`���A1�����I���\�ɂ��܂��B

#### �N���b�N����

- ���I���̍��ڂ��N���b�N: ���̍��ڂ�I��
- �I���ς݂̍��ڂ��N���b�N: `None` �ɖ߂��i�g�O������j

---

## �R�[�f�B���O�K��

### 1. �����K��

#### �N���X��

- �R���|�[�l���g�N���X: `{�@�\��}` (PascalCase)
- �����N���X: `{�R���|�[�l���g��}Attributes` (PascalCase)

#### �萔

- ���ׂđ啶��
- �P����A���_�[�X�R�A�ŋ�؂�
- ��: `COMPONENT_MARGIN_HORIZONTAL`, `TAB_HEIGHT`

#### ���\�b�h

- PascalCase ���g�p
- �����𖾊m�Ɏ����������g�p
- ��: `RenderTab()`, `DrawCheckBox()`, `HandleCheckBoxClick()`

#### �t�B�[���h

- camelCase ���g�p
- private �t�B�[���h�ɂ͐ړ�����t���Ȃ�
- ��: `sectionForcesExpanded`, `filledCheckBox`

### 2. ���[�W�����̎g�p

�֘A����v�f�͕K�����[�W�����ŃO���[�v�����܂��F

**�K�{���[�W����:**
- `UI Layout Constants` - UI�萔�̒�`
- `Fields` - �t�B�[���h�̒�`

**�������[�W����:**
- `Layout Methods` - ���C�A�E�g�v�Z���\�b�h
- `Rendering Methods` - �`�惁�\�b�h
- `Event Handlers` - �C�x���g�������\�b�h

### 3. XML�h�L�������g�R�����g

���ׂĂ�public�����protected���\�b�h��XML�R�����g���L�q���܂��B

**�K�{����:**
- `summary` - ���\�b�h�̐���
- `param` - �p�����[�^�̐����i����ꍇ�j
- `returns` - �߂�l�̐����i����ꍇ�j

### 4. ���\�b�h�̕���

#### �P��ӔC�̌���

�e���\�b�h��1�̖����݂̂����悤�ɕ������܂��F

- `Layout()` - ���C�A�E�g�v�Z�̓���
- `Render()` - �`��̓���
- `RenderTab()` - �^�u�̕`��
- `RenderMenuContent()` - ���j���[�R���e���c�̕`��
- `DrawCheckBox()` - �`�F�b�N�{�b�N�X�̕`��
- `DrawRadioButton()` - ���W�I�{�^���̕`��

#### �w���p�[���\�b�h�̍쐬

���ʏ����͐ϋɓI�Ƀw���p�[���\�b�h�ɒ��o���܂��F

- `CreateControlRect()` - �R���g���[����`�̐���
- `DrawRadioButtonWithLabel()` - ���W�I�{�^���ƃ��x�����Z�b�g�ŕ`��
- `HandleCheckBoxClick()` - �`�F�b�N�{�b�N�X�̃N���b�N����
- `HandleRadioButtonClick()` - ���W�I�{�^���̃N���b�N����

### 5. ���\�[�X�Ǘ�

#### using �X�e�[�g�����g

`IDisposable` ����������I�u�W�F�N�g�͕K�� `using` �ň݂͂܂��F

**�ΏۃI�u�W�F�N�g:**
- `SolidBrush`
- `Pen`
- `Graphics` (����̏ꍇ)
- `Font` (�V�K�쐬�����ꍇ)

#### GH_Capsule �̔j��

`GH_Capsule` �͕K�������I�� `Dispose()` ���Ăяo���܂��B

### 6. �p�t�H�[�}���X�l������

#### �`��̍œK��

- `Render()` ���\�b�h���ŏd�������������
- �\�Ȍ��� `Layout()` �Ōv�Z���ς܂���
- �p�ɂɍ쐬�����I�u�W�F�N�g�͎g���񂵂�����

#### ���������[�N�h�~

- �C�x���g�n���h���̓o�^������Y��Ȃ�
- �傫�ȃI�u�W�F�N�g�͓K�؂ɔj������

---

## ������

### �W�J�\�ȃ^�u���j���[�̊��S����

�Q�l����: `FEMur.GH\Results\SectionForceView.cs`

���̎����ɂ͈ȉ����܂܂�܂��F

1. �W�J�\�ȃ^�u
2. �`�F�b�N�{�b�N�X�i2�j
3. ���W�I�{�^���i6�A1�̂ݑI���\�j
4. �K�؂ȃ��[�W��������
5. �w���p�[���\�b�h�̊��p
6. XML�h�L�������g�R�����g

### ��v���\�b�h�̎����p�^�[��

#### Layout() ���\�b�h

���C�A�E�g�v�Z�̊�{�p�^�[���F

1. `base.Layout()` ���Ăяo��
2. �R���|�[�l���g�̃o�E���h���擾
3. �ǉ��̍������v�Z
4. �^�u�̈ʒu���v�Z
5. �W�J���̓��j���[�v�f�̈ʒu���v�Z

#### Render() ���\�b�h

�`�揈���̊�{�p�^�[���F

1. `base.Render()` ���Ăяo��
2. `GH_CanvasChannel.Objects` �`���l���ŕ`��
3. �^�u��`��
4. �W�J���̓��j���[�R���e���c��`��

#### RespondToMouseDown() ���\�b�h

�C�x���g�����̊�{�p�^�[���F

1. ���N���b�N�̂ݏ���
2. �^�u�N���b�N�œW�J/�܂肽����
3. �W�J���͊e�R���g���[���̃N���b�N������
4. ���������ꍇ�� `GH_ObjectResponse.Handled` ��Ԃ�
5. �������Ȃ��ꍇ�� `base.RespondToMouseDown()` ���Ăяo��

---

## �`�F�b�N���X�g

### �݌v�t�F�[�Y

- [ ] UI�v�f�̔z�u������
- [ ] �K�v�Ȓ萔��􂢏o��
- [ ] �J���[�X�L�[��������
- [ ] ��ԊǗ��̕��@������ibool, enum���j

### �����t�F�[�Y

**�萔��`**
- [ ] ���ׂĂ̒萔�� `#region UI Layout Constants` �ɒ�`
- [ ] �K�؂Ȗ����K���ɏ]���Ă��邩�m�F
- [ ] �J�e�S�����ƂɃO���[�v������Ă��邩�m�F

**�N���X�\��**
- [ ] ���[�W�����ŃR�[�h�𐮗�
- [ ] �t�B�[���h��K�؂ɒ�`
- [ ] �v���p�e�B�� Owner ���`

**���\�b�h����**
- [ ] ���\�b�h��P��ӔC�ɕ���
- [ ] �w���p�[���\�b�h��K�؂Ɋ��p
- [ ] XML�h�L�������g�R�����g���L�q

**���\�[�X�Ǘ�**
- [ ] ���ׂĂ� `using` �X�e�[�g�����g���������g�p����Ă���
- [ ] `GH_Capsule` ���K�؂ɔj������Ă���
- [ ] �C�x���g�n���h�����K�؂ɓo�^/��������Ă���

### �e�X�g�t�F�[�Y

**�@�\�e�X�g**
- [ ] �^�u�̓W�J/�܂肽���݂�����ɓ���
- [ ] ���ׂẴN���b�N�̈悪����������
- [ ] �`�F�b�N�{�b�N�X�̃g�O�����삪����
- [ ] ���W�I�{�^���̔r���I��������

**UI/UX�e�X�g**
- [ ] ���C�A�E�g������Ă��Ȃ����m�F
- [ ] �G���[/�x�����̃p���b�g�F���������\��
- [ ] �e�L�X�g���ǂ݂₷�����m�F
- [ ] �R���g���[���̃T�C�Y���K�؂��m�F

**�p�t�H�[�}���X�e�X�g**
- [ ] �`�揈�����d���Ȃ����m�F
- [ ] ���������[�N���Ȃ����m�F

### �R�[�h���r���[

**�����K��**
- [ ] �N���X�����K�؂�
- [ ] ���\�b�h����������\���Ă��邩
- [ ] �萔�������m��

**�R�����g**
- [ ] XML�h�L�������g�R�����g����������Ă��邩
- [ ] ���G�ȏ����ɐ����R�����g�����邩

**�R�[�h�i��**
- [ ] �d���R�[�h���Ȃ���
- [ ] ���\�b�h���������Ȃ����i�ڈ�: 50�s�ȓ��j
- [ ] �l�X�g���[�����Ȃ����i�ڈ�: 3�K�w�ȓ��j

---

## �悭������Ɖ�����

### ���1: �^�u�N���b�N���ɃT�C�Y���ς��Ȃ�

**����:** `Layout()` ���\�b�h���ČĂяo�����Ă��Ȃ�

**������:** �^�u�N���b�N���� `Layout()` �𖾎��I�ɌĂяo��

### ���2: �`�F�b�N�{�b�N�X���������Ȃ�

**����:** �N���b�N����̋�`�̈悪����������A�܂��̓I�t�Z�b�g������Ă���

**������:** �f�o�b�O���ɋ�`�̈���������Ċm�F

### ���3: �F���������\������Ȃ�

**����:** `GH_Capsule` �̃f�t�H���g�F���㏑������Ă��Ȃ�

**������:** `GH_Capsule.Render()` �̌�ɖ����I�ɐF��`��

### ���4: ���������[�N

**����:** `Dispose()` ���ĂіY��Ă���

**������:** ���ׂĂ� `IDisposable` �I�u�W�F�N�g�� `using` �ň͂�

---

## �Q�l����

### �v���W�F�N�g���̎�����

- `FEMur.GH\Results\SectionForceView.cs` - ���S�Ȏ�����i�W�J�^�u + �`�F�b�N�{�b�N�X + ���W�I�{�^���j
- `FEMur.GH\Results\ResultView.cs` - �V���v���Ȏ�����

### Grasshopper SDK

- [Grasshopper API Documentation](https://developer.rhino3d.com/api/grasshopper/)
- `GH_Component` �N���X
- `GH_ComponentAttributes` �N���X
- `GH_Capsule` �N���X

---

## �X�V����

| ���t | �o�[�W���� | �ύX���e |
|------|-----------|---------|
| 2025-01-13 | 1.0.0 | ���ō쐬 |

---

## �v���Ҍ����K�C�h

���̃h�L�������g�̉��P��Ă͈ȉ��̕��@�ōs���Ă��������F

1. �V����UI�p�^�[����ǉ�����ꍇ�́A��������܂߂�
2. �萔�l��ύX����ꍇ�́A���R�𖾋L����
3. ������͎��ۂɓ��삷��R�[�h���L�ڂ���
4. �X�N���[���V���b�g������Ƃ�蕪����₷��

---

## ���C�Z���X

���̃h�L�������g�� FEMur �v���W�F�N�g�̈ꕔ�ł���A�v���W�F�N�g�Ɠ������C�Z���X���K�p����܂��B