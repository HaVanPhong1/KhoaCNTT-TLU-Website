import { useState, useEffect } from 'react'
import studentApi from '../../api/studentApi'

function SubjectMultiSelect({ values = [], onChange }) {
	const [keyword, setKeyword] = useState('')
	const [subjects, setSubjects] = useState([])

	useEffect(() => {
		studentApi.getSubjects().then(setSubjects).catch(console.error)
	}, [])

	const filtered = subjects.filter(
		(s) =>
			(s.subjectCode + ' ' + s.subjectName)
				.toLowerCase()
				.includes(keyword.toLowerCase()) &&
			!values.includes(s.subjectCode)
	)

	const add = (code) => {
		onChange([...values, code])
		setKeyword('')
	}

	const remove = (code) => {
		onChange(values.filter((v) => v !== code))
	}

	return (
		<div className='border border-gray-300 rounded-lg p-2 bg-white'>
			{/* TAGS */}
			<div className='flex flex-wrap gap-2 mb-2'>
				{values.map((code) => (
					<span
						key={code}
						className='flex items-center gap-1 bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded-full'>
						{code}

						<button
							type='button'
							onClick={() => remove(code)}
							className='text-blue-500 hover:text-red-500'>
							✕
						</button>
					</span>
				))}
			</div>

			{/* SEARCH */}
			<input
				type='text'
				placeholder='Tìm môn học...'
				value={keyword}
				onChange={(e) => setKeyword(e.target.value)}
				className='w-full border-b pb-1 text-sm outline-none'
			/>

			{/* SUGGESTIONS */}
			{keyword && (
				<div className='max-h-40 overflow-y-auto mt-2 border rounded'>
					{filtered.map((s) => (
						<div
							key={s.subjectCode}
							onClick={() => add(s.subjectCode)}
							className='px-2 py-1 cursor-pointer hover:bg-gray-100 text-sm'>
							<span className='font-medium'>{s.subjectCode}</span>{' '}
							<span className='text-gray-500'>– {s.subjectName}</span>
						</div>
					))}
				</div>
			)}
		</div>
	)
}

export default SubjectMultiSelect
