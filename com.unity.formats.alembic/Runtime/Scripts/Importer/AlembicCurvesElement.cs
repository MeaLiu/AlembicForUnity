using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Importer
{
    internal class AlembicCurvesElement : AlembicElement
    {
        // members
        aiCurves m_abcSchema;
        PinnedList<aiCurvesData> m_abcData = new PinnedList<aiCurvesData>(1);
        aiCurvesSummary m_summary;
        aiCurvesSampleSummary m_sampleSummary;

        internal override aiSchema abcSchema { get { return m_abcSchema; } }
        public override bool visibility
        {
            get { return m_abcData[0].visibility; }
        }

        internal override void AbcSetup(aiObject abcObj, aiSchema abcSchema)
        {
            base.AbcSetup(abcObj, abcSchema);
            m_abcSchema = (aiCurves)abcSchema;
            m_abcSchema.GetSummary(ref m_summary);
        }

        public override void AbcPrepareSample()
        {
            base.AbcPrepareSample();
        }

        public override void AbcSyncDataBegin()
        {
            if (!m_abcSchema.schema.isDataUpdated)
                return;

            var sample = m_abcSchema.sample;
            sample.GetSummary(ref m_sampleSummary);

            // get points cloud component
            var curves = abcTreeNode.gameObject.GetComponent<AlembicCurves>();
            if (curves == null)
            {
                curves = abcTreeNode.gameObject.AddComponent<AlembicCurves>();
                //  abcTreeNode.gameObject.AddComponent<AlembicPointsRenderer>(); // Need rendering
            }
            var data = default(aiCurvesData);


            curves.positionsList.ResizeDiscard(m_sampleSummary.positionCount);

            if (m_summary.hasWidths)
            {
                curves.positionOffsetBuffer.ResizeDiscard(m_sampleSummary.numVerticesCount);
                data.positions = curves.positionsList;
                data.numVertices = curves.positionOffsetBuffer;
            }

            if (m_summary.hasWidths)
            {
                curves.widths.ResizeDiscard(m_sampleSummary.positionCount);
                data.widths = curves.widths;
            }

            if (m_summary.hasUVs)
            {
                curves.uvs.ResizeDiscard(m_sampleSummary.positionCount);
                data.uvs = curves.uvs;
            }

            m_abcData[0] = data;

            // kick async copy
            sample.FillData(m_abcData);
        }

        public override void AbcSyncDataEnd()
        {
            if (!m_abcSchema.schema.isDataUpdated)
                return;

            var data = m_abcData[0];

            if (abcTreeNode.stream.streamDescriptor.Settings.ImportVisibility)
                abcTreeNode.gameObject.SetActive(data.visibility);

            var curves = abcTreeNode.gameObject.GetComponent<AlembicCurves>();

            var cnt = 0;
            for (var i = 0; i < curves.PositionsOffsetBuffer.Count; ++i)
            {
                var v = curves.PositionsOffsetBuffer[i];
                curves.PositionsOffsetBuffer[i] = cnt;
                cnt += v;
            }
        }
    }
}
