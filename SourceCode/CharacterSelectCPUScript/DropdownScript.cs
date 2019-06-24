using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownScript : MonoBehaviour
{
    void Start()
    {
        //AIフラグがOFFなら非表示にする
        if (!PlayerManagemaentScript.AI_flag)
            gameObject.SetActive(false);
    }

	public void OnValueChanged(int result)
    {
        //０から始まるので＋１する
        result++;

        PlayerManagemaentScript.AI_level = result;
    }
}
