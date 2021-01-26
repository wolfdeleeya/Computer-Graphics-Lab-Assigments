using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class ComputeShaderParticleSystem : MonoBehaviour
{
    public enum StartVelocityModifiers { Identity, Quadratic}

    [Header("Display Variables")]
    [SerializeField] private float _boundingBoxSize;
    [SerializeField] private Material _material;
    [SerializeField] private Mesh _mesh;

    [Header("Particle System Variables")]
    [SerializeField] private ComputeShader _shader;
    [SerializeField] private int _numOfParticles;
    [SerializeField] private float _lifeLength;
    [SerializeField] private float _velocity;
    [SerializeField] private float _startScale;
    [SerializeField] private float _endScale;
    [SerializeField] private StartVelocityModifiers _modifier;

    private delegate Vector3 StartVelocityModifier(Vector3 randomVelocity);

    private Dictionary<StartVelocityModifiers, StartVelocityModifier> _modifiers = new Dictionary<StartVelocityModifiers, StartVelocityModifier>
    {
        {StartVelocityModifiers.Identity ,(x)=>x},
        {StartVelocityModifiers.Quadratic , (x)=> new Vector3(Mathf.Pow(x.x,2), Mathf.Pow(x.y,2), Mathf.Pow(x.z,2)) }
    };

    struct ParticleStruct
    {
        public Vector3 pos;
        public Vector3 vel;
        public float time;
    };

    private Transform _transform;
    private ComputeBuffer _buffer;
    private static int _calculateKernel;


    private static readonly int _particlesId = Shader.PropertyToID("_Particles"),
        _sourcePosId = Shader.PropertyToID("_SourcePos"),
        _timeToSetId = Shader.PropertyToID("_TimeToSet"),
        _deltaTimeId = Shader.PropertyToID("_DeltaTime"),
        _velId = Shader.PropertyToID("_Vel"),
        _numOfParticlesId = Shader.PropertyToID("_NumOfParticles");

    private void OnEnable()
    {
        _transform = transform;
        _buffer = new ComputeBuffer(_numOfParticles, 28);
        _calculateKernel = _shader.FindKernel("CalculateParticles");
    }

    private void OnDisable()
    {
        _buffer.Release();
        _buffer = null;
    }

    private void Start()
    {
        _material.SetFloat("_StartScale", _startScale);
        _material.SetFloat("_EndScale", _endScale);
        _material.SetFloat(_timeToSetId, _lifeLength);
        _shader.SetVector(_sourcePosId, _transform.position);
        _shader.SetFloat(_timeToSetId, _lifeLength);
        _shader.SetFloat(_velId, _velocity);
        _shader.SetInt(_numOfParticlesId, _numOfParticles);
        InitBuffer();
        _shader.SetBuffer(_calculateKernel, _particlesId, _buffer);
    }

    private void Update()
    {
        _shader.SetVector(_sourcePosId, _transform.position);
        _shader.SetFloat(_deltaTimeId, Time.deltaTime);
        int numOfThreadGroups = Mathf.CeilToInt(_numOfParticles / 64f);
        _shader.Dispatch(_calculateKernel, numOfThreadGroups, 1, 1);
        _material.SetBuffer(_particlesId, _buffer);
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * _boundingBoxSize);
        Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, bounds, _buffer.count);
    }

    private void InitBuffer() 
    {
        ParticleStruct[] data = new ParticleStruct[_numOfParticles];
        for (int i = 0; i < data.Length; ++i) 
        {
            Vector3 randVel = new Vector3(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f);
            data[i].vel = _modifiers[_modifier](randVel).normalized*_velocity;
            data[i].pos = _transform.position;
            data[i].time = _lifeLength;
        }
        _buffer.SetData(data);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(Vector3.zero, Vector3.one * _boundingBoxSize);
    }
}
